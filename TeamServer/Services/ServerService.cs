using System.Reflection;

using AutoMapper;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Drones;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Modules;
using TeamServer.Tasks;

using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Services;

public class ServerService : IServerService
{
    public IDroneService Drones { get; }
    public ITaskService Tasks { get; }
    public ICryptoService Crypto { get; }
    public IReversePortForwardService PortForwards { get; }
    public IHubContext<NotificationHub, INotificationHub> Hub { get; }
    
    private readonly IMapper _mapper;
    private readonly List<ServerModule> _modules = new();

    public ServerService(IDroneService drones, ITaskService tasks, IMapper mapper, ICryptoService crypto, IReversePortForwardService portForwards, IHubContext<NotificationHub,INotificationHub> hub)
    {
        Drones = drones;
        Tasks = tasks;
        Crypto = crypto;
        PortForwards = portForwards;
        Hub = hub;
        
        _mapper = mapper;
        
        LoadModules();
    }
    
    public async Task HandleInboundMessages(IEnumerable<C2Frame> frames)
    {
        foreach (var frame in frames)
            await HandleFrame(frame);
    }
    
    private async Task HandleFrame(C2Frame frame)
    {
        var module = _modules.First(m => m.FrameType == frame.FrameType);
        await module.ProcessFrame(frame);
    }

    public async Task<IEnumerable<C2Frame>> GetOutboundFrames(Metadata metadata)
    {
        // handle check-in first
        await HandleCheckIn(metadata);
        
        var outbound = new List<C2Frame>();
        var pending = (await Tasks.GetPending(metadata.Id)).ToList();

        // if none, send a nop
        if (!pending.Any())
            outbound.Add(new C2Frame(FrameType.NOP));

        foreach (var record in pending)
        {
            // update status and date
            record.Status = TaskStatus.TASKED;
            record.StartTime = DateTime.UtcNow;

            // map it to a task
            var task = _mapper.Map<TaskRecord, DroneTask>(record);

            // encrypt it
            var enc = await Crypto.Encrypt(task);

            // pack into a frame
            outbound.Add(new C2Frame(FrameType.TASK, enc));
            
            // tell hub
            await Hub.Clients.All.TaskUpdated(record.DroneId, record.TaskId);
        }
        
        // update db
        await Tasks.Update(pending);
        
        // add any cached frames
        outbound.AddRange(Tasks.GetCachedFrames(metadata.Id));

        return outbound.ToArray();
    }

    private async Task HandleCheckIn(Metadata metadata)
    {
        var drone = await Drones.Get(metadata.Id);

        if (drone is null)
        {
            drone = new Drone(metadata);

            await Drones.Add(drone);
            await Hub.Clients.All.NewDrone(drone.Metadata.Id);
        }
        else
        {
            drone.CheckIn();

            await Drones.Update(drone);
            await Hub.Clients.All.DroneCheckedIn(drone.Metadata.Id);
        }
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

            module.Init(this);
            _modules.Add(module);
        }
    }
}