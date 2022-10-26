using SQLite;

namespace TeamServer.Storage;

[Table("drones")]
public sealed class DroneDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("parent")]
    public string Parent { get; set; }

    [Column("external_ip")]
    public string ExternalAddress { get; set; }
    
    [Column("internal_ip")]
    public string InternalAddress { get; set; }
    
    [Column("handler")]
    public string Handler { get; set; }
    
    [Column("identity")]
    public string Identity { get; set; }
    
    [Column("hostname")]
    public string Hostname { get; set; }
    
    [Column("process")]
    public string Process { get; set; }
    
    [Column("pid")]
    public int ProcessId { get; set; }
    
    [Column("arch")]
    public int Architecture { get; set; }
    
    [Column("integrity")]
    public int Integrity { get; set; }
    
    [Column("first_seen")]
    public DateTime FirstSeen { get; set; }
    
    [Column("last_seen")]
    public DateTime LastSeen { get; set; }

    [Column("status")]
    public int Status { get; set; }
}