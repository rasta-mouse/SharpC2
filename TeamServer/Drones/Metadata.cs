using ProtoBuf;

namespace TeamServer.Drones;

[ProtoContract]
public sealed class Metadata
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public string Identity { get; set; }
    
    [ProtoMember(3)]
    public byte[] Address { get; set; }
    
    [ProtoMember(4)]
    public string Hostname { get; set; }

    [ProtoMember(5)]
    public string Process { get; set; }
    
    [ProtoMember(6)]
    public int Pid { get; set; }
    
    [ProtoMember(7)]
    public bool Is64Bit { get; set; }
    
    [ProtoMember(8)]
    public IntegrityLevel Integrity { get; set; }
}

public enum IntegrityLevel
{
    MEDIUM,
    HIGH,
    SYSTEM
}