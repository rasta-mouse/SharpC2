using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class RemoveDirectory : DroneCommand
{
    public override byte Command => 0x19;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        Directory.Delete(task.Arguments[0], true);
        Drone.SendTaskComplete(task.Id);
    }
}