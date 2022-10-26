using ProtoBuf;

namespace TeamServer.Models;

[ProtoContract]
public class C2Message
{
    [ProtoMember(1)]
    public string DroneId { get; set; }
    
    [ProtoMember(2)]
    public byte[] Iv { get; set; }
    
    [ProtoMember(3)]
    public byte[] Data { get; set; }
    
    [ProtoMember(4)]
    public byte[] Checksum { get; set; }
}