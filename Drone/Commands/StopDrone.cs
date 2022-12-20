using System.Threading;

namespace Drone.Commands;

public sealed class StopDrone : DroneCommand
{
    public override byte Command => 0x13;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        Drone.Stop();
        Drone.SendTaskComplete(task.Id);
    }
}