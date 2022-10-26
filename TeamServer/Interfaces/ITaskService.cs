using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface ITaskService
{
    // create
    Task AddTask(DroneTaskRecord task);

    // read
    Task<DroneTaskRecord> GetTask(string taskId);
    Task<IEnumerable<DroneTaskRecord>> GetAllTasks();
    Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId);
    Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId, DroneTaskStatus status);

    // update
    Task UpdateTasks(IEnumerable<DroneTaskRecord> tasks);
    Task UpdateTask(DroneTaskRecord task);

    // delete
    Task DeleteTask(DroneTaskRecord task);
}