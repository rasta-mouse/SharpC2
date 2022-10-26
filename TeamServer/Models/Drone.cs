using System.Diagnostics;

namespace TeamServer.Models;

[DebuggerDisplay("{Metadata.Id}")]
public sealed class Drone
{
    public DroneMetadata Metadata { get; set; }
    
    public string Parent { get; set; }
    public string ExternalAddress { get; set; }
    public string Handler { get; set; }
    
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }

    public DroneStatus Status { get; set; }

    public Drone(DroneMetadata metadata)
    {
        Metadata = metadata;
        FirstSeen = LastSeen = DateTime.UtcNow;
    }

    public Drone()
    {
        // automapper
    }

    public void CheckIn()
    {
        LastSeen = DateTime.UtcNow;
    }
}

public enum DroneStatus
{
    ALIVE,
    DEAD
}