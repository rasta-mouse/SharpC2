using SQLite;

namespace TeamServer.Storage;

[Table("smb_handlers")]
public sealed class SmbHandlerDao
{
    [PrimaryKey, Column("name")]
    public string Name { get; set; }
    
    [Column("pipe_name")]
    public string PipeName { get; set; }
}