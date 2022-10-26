using ProtoBuf;

namespace Drone.Models;

[ProtoContract]
public sealed class DroneTaskResponse
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public DroneTaskStatus Status { get; set; }
    
    [ProtoMember(3)]
    public byte Module { get; set; }
    
    [ProtoMember(4)]
    public byte[] Output { get; set; }
}

public enum DroneTaskStatus
{
    Running = 2,
    Complete = 3,
    Aborted = 4
}