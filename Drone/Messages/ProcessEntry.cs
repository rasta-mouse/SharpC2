using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class ProcessEntry
{
    [ProtoMember(1)]
    public int ProcessId { get; set; }
    
    [ProtoMember(2)]
    public int ParentProcessId { get; set; }
    
    [ProtoMember(3)]
    public string Name { get; set; }
    
    [ProtoMember(4)]
    public string Path { get; set; }
    
    [ProtoMember(5)]
    public int SessionId { get; set; }
    
    [ProtoMember(6)]
    public string Owner { get; set; }
    
    [ProtoMember(7)]
    public string Arch { get; set; }
}