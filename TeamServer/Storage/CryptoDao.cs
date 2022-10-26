using SQLite;

namespace TeamServer.Storage;

[Table("crypto")]
public sealed class CryptoDao
{
    [Column("key")]
    public byte[] Key { get; set; }
}