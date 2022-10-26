using TeamServer.Models;

namespace TeamServer.Modules;

public class ScreenshotModule : ServerModule
{
    public override byte Module => 0x09;
    
    public override async Task Execute(DroneTaskOutput output)
    {
        // get the task record
        var task = await Tasks.GetTask(output.TaskId);
        
        // update the task
        task.UpdateTask(output);
        
        // update db
        await Tasks.UpdateTask(task);
        
        // notify hub
        await Hub.Clients.All.NotifyScreenshotAdded(task.DroneId, task.TaskId);
    }
}