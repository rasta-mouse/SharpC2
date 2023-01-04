using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class SocksModule : ServerModule
{
    public override FrameType FrameType => FrameType.SOCKS_PROXY;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        var packet = await Crypto.Decrypt<Socks4Packet>(frame.Data);
        
        if (packet is null)
            return;

        var socks = SocksServers.Get(packet.Id);
        
        if (socks is null)
            return;

        switch (packet.Type)
        {
            case Socks4Packet.PacketType.CONNECT:
            {
                socks.Unblock();
                break;
            }

            case Socks4Packet.PacketType.DATA:
            {
                socks.QueueData(packet.Data);
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}