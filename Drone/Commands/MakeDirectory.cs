using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class MakeDirectory : DroneCommand
{
    public override byte Command => 0x04;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        _ = Directory.CreateDirectory(task.Arguments[0]);
        await Drone.SendTaskComplete(task);
    }
}