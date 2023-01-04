using TeamServer.Interfaces;
using TeamServer.Pivots;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class SocksService : ISocksService
{
    private readonly IDatabaseService _db;
    private readonly List<SocksProxy> _proxies = new();

    public SocksService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task LoadFromDatabase()
    {
        var conn = _db.GetAsyncConnection();
        var socks = await conn.Table<SocksDao>().ToArrayAsync();

        foreach (var dao in socks)
        {
            var sock = (SocksProxy)dao;
            _ = sock.Start();
            
            _proxies.Add(sock);
        }
    }

    public async Task Add(SocksProxy socksProxy)
    {
        _proxies.Add(socksProxy);
        
        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync((SocksDao)socksProxy);
    }

    public IEnumerable<SocksProxy> Get()
    {
        return _proxies;
    }

    public SocksProxy Get(string id)
    {
        return _proxies.FirstOrDefault(s => s.Id.Equals(id));
    }

    public async Task Delete(SocksProxy socksProxy)
    {
        _proxies.Remove(socksProxy);
        
        var conn = _db.GetAsyncConnection();
        await conn.DeleteAsync((SocksDao)socksProxy);
    }
}