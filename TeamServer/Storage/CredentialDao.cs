using SQLite;

namespace TeamServer.Storage;

[Table("credentials")]
public sealed class CredentialDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("domain")]
    public string Domain { get; set; }
    
    [Column("username")]
    public string Username { get; set; }
    
    [Column("password")]
    public string Password { get; set; }
}