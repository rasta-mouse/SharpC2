using SQLite;

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
}