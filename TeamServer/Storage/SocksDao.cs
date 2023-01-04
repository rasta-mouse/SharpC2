using SQLite;

using TeamServer.Pivots;

namespace TeamServer.Storage;

[Table("socks")]
public sealed class SocksDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("drone")]
    public string DroneId { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }

    public static implicit operator SocksDao(SocksProxy socksProxy)
    {
        return new SocksDao
        {
            Id = socksProxy.Id,
            DroneId = socksProxy.DroneId,
            BindPort = socksProxy.BindPort
        };
    }

    public static implicit operator SocksProxy(SocksDao dao)
    {
        if (dao is null)
            return null;
        
        return new SocksProxy
        {
            Id = dao.Id,
            DroneId = dao.DroneId,
            BindPort = dao.BindPort
        };
    }
}