using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Storage;
using TeamServer.Tasks;

using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Services;

public sealed class TaskService : ITaskService
{
    private readonly IDatabaseService _db;

    private readonly Dictionary<string, Queue<C2Frame>> _cached = new();

    public TaskService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task Add(TaskRecord record)
    {
        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync((TaskRecordDao)record);
    }

    public void CacheFrame(C2Frame frame)
    {
        if (!_cached.ContainsKey(frame.DroneId))
            _cached.Add(frame.DroneId, new Queue<C2Frame>());
        
        _cached[frame.DroneId].Enqueue(frame);
    }

    public async Task<IEnumerable<TaskRecord>> GetAll()
    {
        var conn = _db.GetAsyncConnection();
        var records = await conn.Table<TaskRecordDao>().ToArrayAsync();

        return records.Select(r => (TaskRecord)r);
    }

    public async Task<TaskRecord> Get(string droneId, string taskId)
    {
        var conn = _db.GetAsyncConnection();
        
        return await conn.Table<TaskRecordDao>().FirstOrDefaultAsync(r =>
            r.DroneId.Equals(droneId) && r.TaskId.Equals(taskId));
    }

    public async Task<IEnumerable<TaskRecord>> GetAllByDrone(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var records = await conn.Table<TaskRecordDao>().Where(r => r.DroneId.Equals(droneId)).ToArrayAsync();

        return records.Select(r => (TaskRecord)r);
    }

    public async Task<TaskRecord> Get(string taskId)
    {
        var conn = _db.GetAsyncConnection();
        
        return await conn.Table<TaskRecordDao>().FirstOrDefaultAsync(r =>
            r.TaskId.Equals(taskId));
    }

    public async Task<IEnumerable<TaskRecord>> GetPending(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        
        var records = await conn.Table<TaskRecordDao>().Where(r =>
            r.Status == (int)TaskStatus.PENDING).ToArrayAsync();

        return records.Select(r => (TaskRecord)r);
    }

    public IEnumerable<C2Frame> GetCachedFrames(string droneId)
    {
        if (!_cached.ContainsKey(droneId))
            return Array.Empty<C2Frame>();

        List<C2Frame> frames = new();

        while (_cached[droneId].Any())
            frames.Add(_cached[droneId].Dequeue());

        return frames;
    }

    public async Task Update(TaskRecord record)
    {
        var conn = _db.GetAsyncConnection();
        await conn.UpdateAsync((TaskRecordDao)record);
    }

    public async Task Update(IEnumerable<TaskRecord> records)
    {
        var conn = _db.GetAsyncConnection();
        var dao = records.Select(r => (TaskRecordDao)r);

        await conn.UpdateAllAsync(dao);
    }

    public async Task Delete(TaskRecord record)
    {
        var conn = _db.GetAsyncConnection();
        await conn.DeleteAsync((TaskRecordDao)record);
    }
}