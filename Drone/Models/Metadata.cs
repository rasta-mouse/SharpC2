using ProtoBuf;

namespace Drone.Models;

[ProtoContract]
public sealed class Metadata
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public string Address { get; set; }
    
    [ProtoMember(3)]
    public string Identity { get; set; }
    
    [ProtoMember(4)]
    public string Hostname { get; set; }

    [ProtoMember(5)]
    public string Process { get; set; }
    
    [ProtoMember(6)]
    public int ProcessId { get; set; }
    
    [ProtoMember(7)]
    public ProcessIntegrity Integrity { get; set; }
    
    [ProtoMember(8)]
    public ProcessArch Architecture { get; set; }
}

public enum ProcessIntegrity
{
    MEDIUM,
    HIGH,
    SYSTEM
}

public enum ProcessArch
{
    X86,
    X64
}