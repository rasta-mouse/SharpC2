using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public class Socks4Data
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public byte[] Data { get; set; }
}