using SQLite;

using TeamServer.Drones;

namespace TeamServer.Storage;

[Table("drones")]
public sealed class DroneDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }

    [Column("parent")]
    public string Parent { get; set; }

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

    public static implicit operator DroneDao(Drone drone)
    {
        return new DroneDao
        {
            Id = drone.Metadata.Id,
            Parent = drone.Parent,
            Identity = drone.Metadata.Identity,
            Address = drone.Metadata.Address,
            Hostname = drone.Metadata.Hostname,
            Process = drone.Metadata.Process,
            Pid = drone.Metadata.Pid,
            Is64Bit = drone.Metadata.Is64Bit,
            Integrity = (int)drone.Metadata.Integrity,
            FirstSeen = drone.FirstSeen,
            LastSeen = drone.LastSeen,
            Status = (int)drone.Status
        };
    }

    public static implicit operator Drone(DroneDao dao)
    {
        if (dao is null)
            return null;
        
        return new Drone
        {
            Parent = dao.Parent,
            FirstSeen = dao.FirstSeen,
            LastSeen = dao.LastSeen,
            Status = (DroneStatus)dao.Status,
            Metadata = new Metadata
            {
                Id = dao.Id,
                Identity = dao.Identity,
                Address = dao.Address,
                Hostname = dao.Hostname,
                Process = dao.Process,
                Pid = dao.Pid,
                Is64Bit = dao.Is64Bit,
                Integrity = (IntegrityLevel)dao.Integrity
            }
        };
    }
}