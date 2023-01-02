using TeamServer.Drones;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class DroneService : IDroneService
{
    private readonly IDatabaseService _db;

    public DroneService(IDatabaseService db, IPeerToPeerService peerToPeer)
    {
        _db = db;
    }

    public async Task Add(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync((DroneDao)drone);
    }

    public async Task<Drone> Get(string id)
    {
        var conn = _db.GetAsyncConnection();
        return await conn.Table<DroneDao>().FirstOrDefaultAsync(d => d.Id.Equals(id));
    }

    public async Task<IEnumerable<Drone>> Get()
    {
        var conn = _db.GetAsyncConnection();
        var drones = await conn.Table<DroneDao>().ToArrayAsync();

        return drones.Select(d => (Drone)d);
    }

    public async Task Update(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        await conn.UpdateAsync((DroneDao)drone);
    }

    public async Task Delete(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        await conn.DeleteAsync((DroneDao)drone);
    }
}