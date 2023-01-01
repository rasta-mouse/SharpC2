using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class CheckInModule : ServerModule
{
    public override FrameType FrameType => FrameType.CHECK_IN;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        var metadata = await Crypto.Decrypt<Metadata>(frame.Data);

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
}