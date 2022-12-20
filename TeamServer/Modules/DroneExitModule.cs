using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class DroneExitModule : ServerModule
{
    public override FrameType FrameType
        => FrameType.EXIT;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        // recover metadata
        var metadata = await Crypto.Decrypt<Metadata>(frame.Value);
        
        // set drone status
        var drone = await Drones.Get(metadata.Id);
        drone.Status = DroneStatus.DEAD;
        
        // technically we can also check it in
        drone.CheckIn();
        
        await Hub.Clients.All.DroneCheckedIn(drone.Metadata.Id);
        await Hub.Clients.All.DroneExited(drone.Metadata.Id);
    }
}