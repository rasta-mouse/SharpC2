using SQLite;

using TeamServer.Events;

namespace TeamServer.Storage;

[Table("user_auth")]
public sealed class UserAuthDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("nick")]
    public string Nick { get; set; }
    
    [Column("result")]
    public bool Result { get; set; }

    public static implicit operator UserAuthDao(UserAuthEvent ev)
    {
        return new UserAuthDao
        {
            Id = ev.Id,
            Date = ev.Date,
            Nick = ev.Nick,
            Result = ev.Result
        };
    }

    public static implicit operator UserAuthEvent(UserAuthDao dao)
    {
        return new UserAuthEvent
        {
            Id = dao.Id,
            Date = dao.Date,
            Nick = dao.Nick,
            Result = dao.Result
        };
    }
}