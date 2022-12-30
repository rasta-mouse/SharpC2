using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class MakeDirectory : DroneCommand
{
    public override byte Command => 0x18;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(task.Arguments[0]);
        await Drone.SendTaskComplete(task.Id);
    }
}