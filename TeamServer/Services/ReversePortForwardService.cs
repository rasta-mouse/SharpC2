using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Pivots;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class ReversePortForwardService : IReversePortForwardService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;
    
    public ReversePortForwardService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task Add(ReversePortForward fwd)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<ReversePortForward, ReversePortForwardDao>(fwd);
        
        await conn.InsertAsync(dao);
    }

    public async Task<ReversePortForward> Get(string forwardId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<ReversePortForwardDao>()
            .FirstOrDefaultAsync(f => f.Id.Equals(forwardId));
        
        return _mapper.Map<ReversePortForwardDao, ReversePortForward>(dao);
    }

    public async Task<IEnumerable<ReversePortForward>> GetAll()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<ReversePortForwardDao>().ToArrayAsync();
        
        return _mapper.Map<IEnumerable<ReversePortForwardDao>, IEnumerable<ReversePortForward>>(dao);
    }

    public async Task<IEnumerable<ReversePortForward>> GetAll(string droneId)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<ReversePortForwardDao>()
            .Where(f => f.DroneId.Equals(droneId))
            .ToArrayAsync();
        
        return _mapper.Map<IEnumerable<ReversePortForwardDao>, IEnumerable<ReversePortForward>>(dao);
    }

    public async Task Delete(ReversePortForward forward)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<ReversePortForward, ReversePortForwardDao>(forward);

        await conn.DeleteAsync(dao);
    }

    public async Task Delete(IEnumerable<ReversePortForward> forwards)
    {
        var conn = _db.GetAsyncConnection();
        var daos = _mapper.Map<IEnumerable<ReversePortForward>, IEnumerable<ReversePortForwardDao>>(forwards);

        foreach (var dao in daos)
            await conn.DeleteAsync(dao);
    }
}