using SQLite;

namespace TeamServer.Storage;

[Table("ext_handlers")]
public sealed class ExternalHandlerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("payload_type")]
    public int PayloadType { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
}