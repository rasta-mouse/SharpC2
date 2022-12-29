using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public sealed class ReversePortForwardPacket
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public PacketType Type { get; set; }
    
    [ProtoMember(3)]
    public byte[] Data { get; set; }

    public ReversePortForwardPacket(string id, PacketType type, byte[] data)
    {
        Id = id;
        Type = type;
        Data = data;
    }
    
    public ReversePortForwardPacket(string id, PacketType type)
    {
        Id = id;
        Type = type;
    }

    public ReversePortForwardPacket()
    {
        
    }

    public enum PacketType
    {
        START,
        DATA,
        STOP
    }
}