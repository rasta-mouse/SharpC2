using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class ReversePortForwardPacket
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public PacketType Type { get; set; }
    
    [ProtoMember(3)]
    public byte[] Data { get; set; }

    public enum PacketType
    {
        START,
        DATA,
        STOP
    }
}