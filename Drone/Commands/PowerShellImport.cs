using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class PowerShellImport : DroneCommand
{
    public override byte Command => 0x0F;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        ImportedScript = Encoding.ASCII.GetString(task.Artefact);
        await Drone.SendTaskComplete(task);
    }

    public static string ImportedScript { get; private set; }
}