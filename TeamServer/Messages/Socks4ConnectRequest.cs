using System.Net;
using System.Text;

using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public sealed class Socks4ConnectRequest
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    public int Version { get; set; }
    public CommandCode Command { get; set; }
    
    [ProtoMember(2)]
    public int DestinationPort { get; set; }
    
    [ProtoMember(3)]
    public byte[] DestinationAddress { get; set; }
    
    [ProtoMember(4)]
    public string DestinationDomain { get; set; }

    public Socks4ConnectRequest(byte[] data)
    {
        Version = Convert.ToInt32(data[0]);
        Command = (CommandCode)data[1];
        DestinationPort = data[3] | data[2] << 8;
        
        var address = new IPAddress(data[4..8]);
        DestinationAddress = address.GetAddressBytes();
            
        // if this is SOCKS4a
        if (address.ToString().StartsWith("0.0.0."))
            DestinationDomain = Encoding.UTF8.GetString(data[9..]);
    }

    public enum CommandCode : byte
    {
        StreamConnection = 0x01,
        PortBinding = 0x02
    }
}