using TeamServer.Models;

namespace TeamServer.Modules;

public class ProcessListingModule : ServerModule
{
    public override byte Module => 0x0B;
    
    public override async Task Execute(DroneTaskOutput output)
    {
        // get the task record
        var task = await Tasks.GetTask(output.TaskId);

        // update the task
        task.UpdateTask(output);

        // update db
        await Tasks.UpdateTask(task);

        // notify hub
        await Hub.Clients.All.NotifyProcessListing(task.DroneId, task.TaskId);
    }
}