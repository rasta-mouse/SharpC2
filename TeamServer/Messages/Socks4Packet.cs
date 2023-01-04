using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public sealed class Socks4Packet
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public PacketType Type { get; set; }
    
    [ProtoMember(3)]
    public byte[] Data { get; set; }

    public Socks4Packet(string id, PacketType type, byte[] data = null)
    {
        Id = id;
        Type = type;
        Data = data;
    }

    public Socks4Packet()
    {
        
    }

    public enum PacketType
    {
        CONNECT,
        DATA,
        DISCONNECT
    }
}