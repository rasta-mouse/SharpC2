using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class Exit : DroneCommand
{
    public override byte Command => 0x22;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        await Drone.Stop(task);
    }
}