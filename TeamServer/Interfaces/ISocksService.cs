using TeamServer.Pivots;

namespace TeamServer.Interfaces;

public interface ISocksService
{
    Task LoadFromDatabase();
    
    Task Add(SocksProxy socksProxy);

    IEnumerable<SocksProxy> Get();
    SocksProxy Get(string id);

    Task Delete(SocksProxy socksProxy);
}