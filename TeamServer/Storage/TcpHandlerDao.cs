using SQLite;

namespace TeamServer.Storage;

[Table("tcp_handlers")]
public sealed class TcpHandlerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("payload_type")]
    public int PayloadType { get; set; }
    
    [Column("address")]
    public string Address { get; set; }
    
    [Column("port")]
    public int Port { get; set; }
    
    [Column("loopback")]
    public bool Loopback { get; set; }
}