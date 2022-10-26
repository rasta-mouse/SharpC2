using System.Collections.Concurrent;

using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Services;

public class TaskService : ITaskService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;
    
    public TaskService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task AddTask(DroneTaskRecord task)
    {
        var conn = _db.GetAsyncConnection();

        var dao = _mapper.Map<DroneTaskRecord, DroneTaskRecordDao>(task);
        await conn.InsertAsync(dao);
    }

    public async Task<DroneTaskRecord> GetTask(string taskId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneTaskRecordDao>()
            .FirstOrDefaultAsync(t => t.TaskId.Equals(taskId));

        return _mapper.Map<DroneTaskRecordDao, DroneTaskRecord>(dao);
    }

    public async Task<IEnumerable<DroneTaskRecord>> GetAllTasks()
    {
        var conn = _db.GetAsyncConnection();
        var query = conn.Table<DroneTaskRecordDao>();
        var dao = await query.ToArrayAsync();

        return _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTaskRecord>>(dao);
    }

    public async Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var query = conn.Table<DroneTaskRecordDao>()
            .Where(t => t.DroneId.Equals(droneId));
        
        var dao = await query.ToArrayAsync();

        return _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTaskRecord>>(dao);
    }

    public async Task<IEnumerable<DroneTaskRecord>> GetTasks(string droneId, DroneTaskStatus status)
    {
        var conn = _db.GetAsyncConnection();

        var query = conn.Table<DroneTaskRecordDao>()
            .Where(t => t.DroneId.Equals(droneId) && t.Status == (int)status);

        var dao = await query.ToArrayAsync();

        return _mapper.Map<IEnumerable<DroneTaskRecordDao>, IEnumerable<DroneTaskRecord>>(dao);
    }

    public async Task UpdateTasks(IEnumerable<DroneTaskRecord> tasks)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<IEnumerable<DroneTaskRecord>, IEnumerable<DroneTaskRecordDao>>(tasks);

        await conn.UpdateAllAsync(dao);
    }

    public async Task UpdateTask(DroneTaskRecord task)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<DroneTaskRecord, DroneTaskRecordDao>(task);

        await conn.UpdateAsync(dao);
    }

    public async Task DeleteTask(DroneTaskRecord task)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<DroneTaskRecord, DroneTaskRecordDao>(task);

        await conn.DeleteAsync(dao);
    }
}