namespace TeamServer.Interfaces;

public interface IHubService
{
    // handlers
    Task NotifyHttpHandlerCreated(string name);
    Task NotifyHttpHandlerDeleted(string name);
    Task NotifyTcpHandlerCreated(string name);
    Task NotifyTcpHandlerDeleted(string name);
    Task NotifySmbHandlerCreated(string name);
    Task NotifySmbHandlerDeleted(string name);

    // drones
    Task NotifyNewDrone(string id);
    Task NotifyDroneCheckedIn(string id);
    Task NotifyDroneRemoved(string id);
    Task NotifyDroneStatusChanged(string id, int status);

    // tasks
    Task NotifyDroneTasked(string drone, string alias, string[] arguments, string artefactPath);
    Task NotifySentDroneData(string drone, int size);
    Task NotifyDroneTaskUpdated(string droneId, string taskId);
    Task NotifyDirectoryListing(string droneId, string taskId);
    Task NotifyProcessListing(string droneId, string taskId);
    Task NotifyScreenshotAdded(string droneId, string taskId);
    
    // credentials
    Task NotifyCredentialAdded(string id);
    Task NotifyCredentialUpdated(string id);
    Task NotifyCredentialDeleted(string id);
}