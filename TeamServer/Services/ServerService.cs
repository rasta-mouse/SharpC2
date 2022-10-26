using System.Reflection;

using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Modules;
using TeamServer.Utilities;

namespace TeamServer.Services;

public class ServerService : IServerService
{
    private readonly IDroneService _drones;
    private readonly ITaskService _tasks;
    private readonly ICredentialService _credentials;
    private readonly ICryptoService _crypto;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    private readonly List<ServerModule> _modules = new();

    public ServerService(IDroneService drones, ITaskService tasks, ICredentialService credentials,
        ICryptoService crypto, IHubContext<HubService, IHubService> hub, IMapper mapper)
    {
        _drones = drones;
        _tasks = tasks;
        _credentials = credentials;
        _crypto = crypto;
        _hub = hub;
        _mapper = mapper;

        LoadModules();
    }

    public async Task HandleInboundMessages(IEnumerable<C2Message> messages)
    {
        foreach (var message in messages)
            await HandleInboundMessage(message);
    }

    private async Task HandleInboundMessage(C2Message message)
    {
        // do a check-in here for p2p drones
        var drone = await _drones.GetDrone(message.DroneId);

        if (drone is not null)
        {
            drone.CheckIn();
            
            await _drones.UpdateDrone(drone);
            await _hub.Clients.All.NotifyDroneCheckedIn(drone.Metadata.Id);
        }
        
        var responses = await _crypto.DecryptObject<IEnumerable<DroneTaskOutput>>(
            message.Iv,
            message.Data,
            message.Checksum);

        await HandleTaskResponses(responses);
    }

    private async Task HandleTaskResponses(IEnumerable<DroneTaskOutput> outputs)
    {
        foreach (var output in outputs)
            await HandleTaskResponse(output);
    }

    private async Task HandleTaskResponse(DroneTaskOutput output)
    {
        var module = _modules.FirstOrDefault(m => m.Module == output.Module);

        if (module is null)
        {
            Console.WriteLine($"[!] Server Module \"{output.Module}\" not found.");
            return;
        }

        // execute module
        await module.Execute(output);
    }

    public async Task<byte[]> GetOutboundMessages(string egressDrone)
    {
        // create list of messages
        var outbound = new List<C2Message>();
        
        // get the egress drone
        var drone = await _drones.GetDrone(egressDrone);

        if (drone is null)
            return Array.Empty<byte>();

        // check-in the drone
        drone.CheckIn();
        await _drones.UpdateDrone(drone);
        await _hub.Clients.All.NotifyDroneCheckedIn(drone.Metadata.Id);

        // get messages for each drone in path
        var vertexes = _drones.DepthFirstSearch(drone.Metadata.Id);

        foreach (var vertex in vertexes)
        {
            var message = await GetDroneMessage(vertex); 
            
            if (message is not null)
                outbound.Add(message);
        }
        
        var raw = outbound.Serialize();
        
        // notify hub
        if (raw.Length > 0)
            await _hub.Clients.All.NotifySentDroneData(drone.Metadata.Id, raw.Length);

        // return messages
        return raw;
    }

    private async Task<C2Message> GetDroneMessage(string droneId)
    {
        // get pending task records
        var records = (await _tasks.GetTasks(droneId, DroneTaskStatus.Pending)).ToList();

        // if no tasks, return null
        if (!records.Any())
            return null;

        // update task status
        records.ForEach(t => t.Status = DroneTaskStatus.Tasked);
        records.ForEach(t => t.StartTime = DateTime.UtcNow);
        await _tasks.UpdateTasks(records);

        // notify hub
        foreach (var task in records)
            await _hub.Clients.All.NotifyDroneTaskUpdated(droneId, task.TaskId);

        // transform into drone tasks
        var tasks = _mapper.Map<IEnumerable<DroneTaskRecord>, IEnumerable<DroneTask>>(records);

        // encrypt
        var (iv, data, checksum) = await _crypto.EncryptObject(tasks);

        // return message
        return new C2Message
        {
            DroneId = droneId,
            Iv = iv,
            Data = data,
            Checksum = checksum
        };
    }

    private void LoadModules()
    {
        var self = Assembly.GetExecutingAssembly();

        foreach (var type in self.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(ServerModule)))
                continue;

            var module = (ServerModule)Activator.CreateInstance(type);
            
            if (module is null)
                continue;
            
            module.Init(_drones, _tasks, _credentials, _hub);
            _modules.Add(module);
        }
    }
}