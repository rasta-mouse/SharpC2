using SQLite;

namespace TeamServer.Storage;

[Table("drones")]
public sealed class DroneDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("identity")]
    public string Identity { get; set; }
    
    [Column("address")]
    public byte[] Address { get; set; }
    
    [Column("hostname")]
    public string Hostname { get; set; }
    
    [Column("process")]
    public string Process { get; set; }
    
    [Column("pid")]
    public int Pid { get; set; }
    
    [Column("arch")]
    public bool Is64Bit { get; set; }
    
    [Column("integrity")]
    public int Integrity { get; set; }
    
    [Column("first_seen")]
    public DateTime FirstSeen { get; set; }
    
    [Column("last_seen")]
    public DateTime LastSeen { get; set; }

    [Column("status")]
    public int Status { get; set; }
}