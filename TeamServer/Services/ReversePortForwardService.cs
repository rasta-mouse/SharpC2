using TeamServer.Interfaces;
using TeamServer.Pivots;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class ReversePortForwardService : IReversePortForwardService
{
    private readonly IDatabaseService _db;

    public ReversePortForwardService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task Add(ReversePortForward fwd)
    {
        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync((ReversePortForwardDao)fwd);
    }

    public async Task<ReversePortForward> Get(string forwardId)
    {
        var conn = _db.GetAsyncConnection();
        
        return await conn.Table<ReversePortForwardDao>()
            .FirstOrDefaultAsync(f => f.Id.Equals(forwardId));
    }

    public async Task<IEnumerable<ReversePortForward>> GetAll()
    {
        var conn = _db.GetAsyncConnection();
        var forwards = await conn.Table<ReversePortForwardDao>().ToArrayAsync();

        return forwards.Select(f => (ReversePortForward)f);
    }

    public async Task<IEnumerable<ReversePortForward>> GetAll(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var forwards = await conn.Table<ReversePortForwardDao>()
            .Where(f => f.DroneId.Equals(droneId))
            .ToArrayAsync();

        return forwards.Select(f => (ReversePortForward)f);
    }

    public async Task Delete(ReversePortForward forward)
    {
        var conn = _db.GetAsyncConnection();
        await conn.DeleteAsync((ReversePortForwardDao)forward);
    }

    public async Task Delete(IEnumerable<ReversePortForward> forwards)
    {
        var conn = _db.GetAsyncConnection();

        foreach (var forward in forwards)
            await conn.DeleteAsync((ReversePortForwardDao)forward);
    }
}