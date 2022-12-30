using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class PrintWorkingDirectory : DroneCommand
{
    public override byte Command => 0x14;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var directory = Directory.GetCurrentDirectory();
        await Drone.SendTaskOutput(task.Id, directory);
    }
}