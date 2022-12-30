using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class StopDrone : DroneCommand
{
    public override byte Command => 0x13;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        await Drone.Stop();
    }
}