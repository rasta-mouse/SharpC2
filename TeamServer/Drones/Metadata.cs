using ProtoBuf;

using SharpC2.API.Responses;

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

    public static implicit operator MetadataResponse(Metadata metadata)
    {
        return new MetadataResponse
        {
            Id = metadata.Id,
            Identity = metadata.Identity,
            Address = metadata.Address,
            Hostname = metadata.Hostname,
            Process = metadata.Process,
            Pid = metadata.Pid,
            Is64Bit = metadata.Is64Bit,
            Integrity = (int)metadata.Integrity
        };
    }
}

public enum IntegrityLevel
{
    MEDIUM,
    HIGH,
    SYSTEM
}