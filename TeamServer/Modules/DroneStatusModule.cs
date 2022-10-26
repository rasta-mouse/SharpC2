using TeamServer.Models;

namespace TeamServer.Modules;

public class DroneStatusModule : ServerModule
{
    public override byte Module => 0x22;
    
    public override async Task Execute(DroneTaskOutput output)
    {
        // get the task record
        var task = await Tasks.GetTask(output.TaskId);

        // update the task
        task.UpdateTask(output);

        // update db
        await Tasks.UpdateTask(task);

        // notify hub
        await Hub.Clients.All.NotifyDroneTaskUpdated(task.DroneId, task.TaskId);
        
        // get the drone
        var drone = await Drones.GetDrone(task.DroneId);
        
        // set status to dead
        drone.Status = DroneStatus.DEAD;
        
        // update drone
        await Drones.UpdateDrone(drone);
        
        // notify hub
        await Hub.Clients.All.NotifyDroneStatusChanged(drone.Metadata.Id, (int)drone.Status);
        
        // remove edge
        if (!string.IsNullOrWhiteSpace(drone.Parent))
            Drones.DeleteEdge(drone.Parent, drone.Metadata.Id);

        // remove vertex
        Drones.DeleteVertex(drone.Metadata.Id);
    }
}