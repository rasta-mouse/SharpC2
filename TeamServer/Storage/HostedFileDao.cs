using SQLite;

using TeamServer.Handlers;

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

    public static implicit operator HostedFileDao(HostedFile file)
    {
        return new HostedFileDao
        {
            Id = file.Id,
            Handler = file.Handler,
            Uri = file.Uri,
            Filename = file.Filename,
            Size = file.Size
        };
    }

    public static implicit operator HostedFile(HostedFileDao dao)
    {
        return new HostedFile
        {
            Id = dao.Id,
            Handler = dao.Handler,
            Uri = dao.Uri,
            Filename = dao.Filename,
            Size = dao.Size
        };
    }
}