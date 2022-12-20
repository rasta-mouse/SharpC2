using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class ReadFile : DroneCommand
{
    public override byte Command => 0x16;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var text = File.ReadAllText(task.Arguments[0]);
        Drone.SendTaskOutput(task.Id, text);
    }
}