using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class ExitModule : ServerModule
{
    public override FrameType FrameType
        => FrameType.EXIT;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        // set drone status
        var drone = await Drones.Get(frame.DroneId);
        drone.Status = DroneStatus.DEAD;

        // technically we can also check it in
        drone.CheckIn();
        
        // update db
        await Drones.Update(drone);
        
        await Hub.Clients.All.DroneCheckedIn(drone.Metadata.Id);
        await Hub.Clients.All.DroneExited(drone.Metadata.Id);
    }
}