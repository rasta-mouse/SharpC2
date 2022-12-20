using SQLite;

namespace TeamServer.Storage;

[Table("hosted_files")]
public sealed class HostedFileDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("handler")]
    public string Handler { get; set; }
    
    [Column("uri")]
    public string Uri { get; set; }
    
    [Column("filename")]
    public string Filename { get; set; }
    
    [Column("size")]
    public long Size { get; set; }
}