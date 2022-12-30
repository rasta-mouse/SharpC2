using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class RemoveDirectory : DroneCommand
{
    public override byte Command => 0x19;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        Directory.Delete(task.Arguments[0], true);
        await Drone.SendTaskComplete(task.Id);
    }
}