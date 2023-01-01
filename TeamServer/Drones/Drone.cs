using System.Diagnostics;

namespace TeamServer.Drones;

[DebuggerDisplay("{Metadata.Id}")]
public sealed class Drone
{
    public Metadata Metadata { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public string Parent { get; set; }
    public DroneStatus Status { get; set; }

    public Drone(Metadata metadata)
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
    LOST,
    DEAD
}