using SQLite;

using TeamServer.Events;

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

    public static implicit operator WebLogDao(WebLogEvent ev)
    {
        return new WebLogDao
        {
            Id = ev.Id,
            Date = ev.Date,
            Method = ev.Method,
            Uri = ev.Uri,
            UserAgent = ev.UserAgent,
            SourceAddress = ev.SourceAddress,
            ResponseCode = ev.ResponseCode
        };
    }

    public static implicit operator WebLogEvent(WebLogDao dao)
    {
        return new WebLogEvent
        {
            Id = dao.Id,
            Date = dao.Date,
            Method = dao.Method,
            Uri = dao.Uri,
            UserAgent = dao.UserAgent,
            SourceAddress = dao.SourceAddress,
            ResponseCode = dao.ResponseCode
        };
    }
}