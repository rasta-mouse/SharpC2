using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class UnlinkModule : ServerModule
{
    public override FrameType FrameType => FrameType.UNLINK;
    public override async Task ProcessFrame(C2Frame frame)
    {
        var childId = await Crypto.Decrypt<string>(frame.Data);
        var child = await Drones.Get(childId);

        if (child is not null)
        {
            child.Status = DroneStatus.LOST;
            
            await Drones.Update(child);
            await Hub.Clients.All.DroneLost(childId);
        }
    }
}