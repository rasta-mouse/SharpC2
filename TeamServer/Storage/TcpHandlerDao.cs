using SQLite;

using TeamServer.Handlers;

namespace TeamServer.Storage;

[Table("tcp_handlers")]
public sealed class TcpHandlerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("address")]
    public string Address { get; set; }
    
    [Column("port")]
    public int Port { get; set; }
    
    [Column("loopback")]
    public bool Loopback { get; set; }
    
    [Column("payload_type")]
    public int PayloadType { get; set; }

    public static implicit operator TcpHandlerDao(TcpHandler handler)
    {
        return new TcpHandlerDao
        {
            Id = handler.Id,
            Name = handler.Name,
            Address = handler.Address,
            Port = handler.Port,
            Loopback = handler.Loopback,
            PayloadType = (int)handler.PayloadType
        };
    }

    public static implicit operator TcpHandler(TcpHandlerDao dao)
    {
        return new TcpHandler
        {
            Id = dao.Id,
            Name = dao.Name,
            Address = dao.Address,
            Port = dao.Port,
            Loopback = dao.Loopback,
            PayloadType = (PayloadType)dao.PayloadType
        };
    }
}