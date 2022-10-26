using ProtoBuf;

namespace TeamServer.Models;

[ProtoContract]
public class DroneTaskOutput
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