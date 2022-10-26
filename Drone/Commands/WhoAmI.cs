using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class WhoAmI : DroneCommand
{
    public override byte Command => 0x0C;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var identity = WindowsIdentity.GetCurrent();
        await Drone.SendOutput(task, identity.Name);
    }
}