using SQLite;

namespace TeamServer.Storage;

[Table("rportfwds")]
public sealed class ReversePortForwardDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("drone")]
    public string DroneId { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
    
    [Column("forward_host")]
    public string ForwardHost { get; set; }
    
    [Column("forward_port")]
    public int ForwardPort { get; set; }
}