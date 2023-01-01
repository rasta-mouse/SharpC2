using SQLite;

namespace TeamServer.Storage;

[Table("smb_handlers")]
public sealed class SmbHandlerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("payload_type")]
    public int PayloadType { get; set; }

    [Column("pipe_name")]
    public string PipeName { get; set; }
}