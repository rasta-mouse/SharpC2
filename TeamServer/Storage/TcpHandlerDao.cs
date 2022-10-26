using SQLite;

namespace TeamServer.Storage;

[Table("tcp_handlers")]
public sealed class TcpHandlerDao
{
    [PrimaryKey, Column("name")]
    public string Name { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
    
    [Column("loopback_only")]
    public bool LoopbackOnly { get; set; }
}