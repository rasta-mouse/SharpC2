using TeamServer.Events;

namespace TeamServer.Hubs;

public interface INotificationHub
{
    #region Handlers

    Task HttpHandlerCreated(string name);
    Task HttpHandlerDeleted(string name);

    #endregion

    #region HostedFiles

    Task HostedFileAdded(string id);
    Task HostedFileDeleted(string id);
    
    #endregion

    #region Drones

    Task NewDrone(string id);
    Task DroneCheckedIn(string id);
    Task DroneExited(string id);
    Task DroneDeleted(string id);

    #endregion

    #region Tasks

    Task DroneTasked(string drone, string task);
    Task TaskUpdated(string drone, string task);
    Task TaskDeleted(string drone, string task);

    #endregion

    #region Events

    Task NewEvent(EventType type, string id);

    #endregion
}