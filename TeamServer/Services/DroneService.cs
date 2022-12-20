using AutoMapper;

using TeamServer.Drones;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class DroneService : IDroneService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    public DroneService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task Add(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.InsertAsync(dao);
    }

    public async Task<Drone> Get(string id)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().FirstOrDefaultAsync(d => d.Id.Equals(id));

        return _mapper.Map<DroneDao, Drone>(dao);
    }

    public async Task<IEnumerable<Drone>> Get()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().ToArrayAsync();

        return _mapper.Map<IEnumerable<DroneDao>, IEnumerable<Drone>>(dao);
    }

    public async Task Update(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);

        await conn.UpdateAsync(dao);
    }

    public async Task Delete(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);

        await conn.DeleteAsync(dao);
    }
}