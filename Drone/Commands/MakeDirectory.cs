using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class MakeDirectory : DroneCommand
{
    public override byte Command => 0x18;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(task.Arguments[0]);
        Drone.SendTaskComplete(task.Id);
    }
}