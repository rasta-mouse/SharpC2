using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Storage;
using TeamServer.Tasks;

using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Services;

public sealed class TaskService : ITaskService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;
    
    private readonly Dictionary<string, Queue<C2Frame>> _cached = new();

    public TaskService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task Add(TaskRecord record)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<TaskRecord, TaskRecordDao>(record);

        await conn.InsertAsync(dao);
    }

    public void CacheFrame(string droneId, C2Frame frame)
    {
        if (!_cached.ContainsKey(droneId))
            _cached.Add(droneId, new Queue<C2Frame>());
        
        _cached[droneId].Enqueue(frame);
    }

    public async Task<IEnumerable<TaskRecord>> GetAll()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<TaskRecordDao>().ToArrayAsync();

        return _mapper.Map<IEnumerable<TaskRecordDao>, IEnumerable<TaskRecord>>(dao);
    }

    public async Task<TaskRecord> Get(string droneId, string taskId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<TaskRecordDao>().FirstOrDefaultAsync(r => r.DroneId.Equals(droneId)
                                                                             && r.TaskId.Equals(taskId));

        return _mapper.Map<TaskRecordDao, TaskRecord>(dao);
    }

    public async Task<IEnumerable<TaskRecord>> GetAllByDrone(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<TaskRecordDao>().Where(r => r.DroneId.Equals(droneId)).ToArrayAsync();

        return _mapper.Map<IEnumerable<TaskRecordDao>, IEnumerable<TaskRecord>>(dao);
    }

    public async Task<TaskRecord> Get(string taskId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<TaskRecordDao>().FirstOrDefaultAsync(r => r.TaskId.Equals(taskId));

        return _mapper.Map<TaskRecordDao, TaskRecord>(dao);
    }

    public async Task<IEnumerable<TaskRecord>> GetPending(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<TaskRecordDao>().Where(r => r.Status == (int)TaskStatus.PENDING).ToArrayAsync();

        return _mapper.Map<IEnumerable<TaskRecordDao>, IEnumerable<TaskRecord>>(dao);
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
        var dao = _mapper.Map<TaskRecord, TaskRecordDao>(record);

        await conn.UpdateAsync(dao);
    }

    public async Task Update(IEnumerable<TaskRecord> records)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<IEnumerable<TaskRecord>, IEnumerable<TaskRecordDao>>(records);

        await conn.UpdateAllAsync(dao);
    }

    public async Task Delete(TaskRecord record)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<TaskRecord, TaskRecordDao>(record);

        await conn.DeleteAsync(dao);
    }
}