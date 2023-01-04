using TeamServer.Messages;
using TeamServer.Tasks;

namespace TeamServer.Interfaces;

public interface ITaskService
{
    // create
    Task Add(TaskRecord record);
    void CacheFrame(C2Frame frame);
    
    // read
    Task<IEnumerable<TaskRecord>> GetAll();
    Task<IEnumerable<TaskRecord>> GetAllByDrone(string droneId);
    Task<TaskRecord> Get(string taskId);

    Task<IEnumerable<TaskRecord>> GetPending(string droneId);
    IEnumerable<C2Frame> GetCachedFrames(string droneId);

    // update
    Task Update(TaskRecord record);
    Task Update(IEnumerable<TaskRecord> records);
    
    // delete
    Task Delete(TaskRecord record);
}