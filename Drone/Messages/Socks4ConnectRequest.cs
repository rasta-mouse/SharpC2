using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class Socks4ConnectRequest
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public int DestinationPort { get; set; }
    
    [ProtoMember(3)]
    public byte[] DestinationAddress { get; set; }
    
    [ProtoMember(4)]
    public string DestinationDomain { get; set; }
}