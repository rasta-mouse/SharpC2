using TeamServer.Models;
using TeamServer.Utilities;

namespace TeamServer.Modules;

public class PeerToPeerModule : ServerModule
{
    public override byte Module => 0x1E;
    
    public override async Task Execute(DroneTaskOutput output)
    {
        // get drone metadata
        var metadata = output.Output.Deserialize<DroneMetadata>();
        
        if (metadata is null)
            return;

        // get the connect/link task
        var task = await Tasks.GetTask(output.TaskId);
        
        // get the parent
        var parent = await Drones.GetDrone(task.DroneId);
        
        // check if we've seen this child before
        var child = await Drones.GetDrone(metadata.Id);
        
        // if not found, it's a new drone
        if (child is null)
        {
            // create child drone
            child = new Drone(metadata)
            {
                Parent = parent.Metadata.Id
            };

            // add it
            await Drones.AddDrone(child);
            
            // add new p2p vertex & edge
            Drones.AddVertex(child.Metadata.Id);
            Drones.AddEdge(parent.Metadata.Id, child.Metadata.Id);
            
            // notify hub
            await Hub.Clients.All.NotifyNewDrone(child.Metadata.Id);
            
            return;
        }
        
        // otherwise remove the old edge
        var oldParent = await Drones.GetDrone(child.Parent);
        Drones.DeleteEdge(oldParent.Metadata.Id, child.Metadata.Id);
        
        // and update the parent
        child.Parent = parent.Metadata.Id;
        await Drones.UpdateDrone(child);
        Drones.AddEdge(parent.Metadata.Id, child.Metadata.Id);
    }
}