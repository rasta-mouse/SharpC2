using SQLite;

namespace TeamServer.Storage;

[Table("web_log")]
public sealed class WebLogDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("method")]
    public string Method { get; set; }
    
    [Column("uri")]
    public string Uri { get; set; }
    
    [Column("user_agent")]
    public string UserAgent { get; set; }
    
    [Column("src_ip")]
    public string SourceAddress { get; set; }
    
    [Column("response")]
    public int ResponseCode { get; set; }
}