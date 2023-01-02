using System.Diagnostics;

using SharpC2.API.Responses;

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
        
    }

    public void CheckIn()
    {
        LastSeen = DateTime.UtcNow;
    }

    public static implicit operator DroneResponse(Drone drone)
    {
        return new DroneResponse
        {
            Metadata = drone.Metadata,
            FirstSeen = drone.FirstSeen,
            LastSeen = drone.LastSeen,
            Parent = drone.Parent,
            Status = (int)drone.Status
        };
    }
}

public enum DroneStatus
{
    ALIVE,
    LOST,
    DEAD
}