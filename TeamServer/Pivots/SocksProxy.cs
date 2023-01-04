using System.Net;
using System.Net.Sockets;

using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Utilities;

namespace TeamServer.Pivots;

public sealed class SocksProxy
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }

    private readonly ICryptoService _crypto;
    private readonly ITaskService _tasks;
    
    private readonly CancellationTokenSource _tokenSource = new();
    private readonly ManualResetEvent _signal = new(false);
    private readonly Queue<byte[]> _dataQueue = new();

    public SocksProxy()
    {
        _crypto = Program.GetService<ICryptoService>();
        _tasks = Program.GetService<ITaskService>();
    }

    public async Task Start()
    {
        var listener = new TcpListener(new IPEndPoint(IPAddress.Any, BindPort));
        listener.Start(100);

        while (!_tokenSource.IsCancellationRequested)
        {
            // wait for client
            // this will throw an OperationCancelledException if the token is cancelled
            try
            {
                var client = await listener.AcceptTcpClientAsync(_tokenSource.Token);
            
                // handle client in new thread
                var thread = new Thread(HandleClient);
                thread.Start(client);
            }
            catch (OperationCanceledException)
            {
                // ignore and proceed to stop the listener
            }
        }
        
        listener.Stop();
    }

    public void QueueData(byte[] data)
    {
        _dataQueue.Enqueue(data);
    }

    public void Unblock()
    {
        _signal.Set();
    }

    private async void HandleClient(object obj)
    {
        if (obj is not TcpClient client)
            return;
        
        var stream = client.GetStream();
        
        // first thing is to read the connect request
        var connectReq = await ReadConnectRequest(stream);
        
        // if not version 4, send error
        if (connectReq.Version != 4)
        {
            await SendConnectReply(stream, false);
            return;
        }
        
        // otherwise, send "connect" task to drone
        var packet = new Socks4Packet(Id, Socks4Packet.PacketType.CONNECT, connectReq.Serialize());
        var frame = new C2Frame(DroneId, FrameType.SOCKS_PROXY, await _crypto.Encrypt(packet));
        _tasks.CacheFrame(frame);
            
        // wait for confirmation from drone
        _signal.WaitOne();
        
        // send success back to client
        await SendConnectReply(stream, true);

        // drop into a loop
        while (!_tokenSource.IsCancellationRequested)
        {
            // if client has data
            if (client.DataAvailable())
            {
                // read it
                var data = await stream.ReadStream();
                
                // send to the drone
                packet = new Socks4Packet(Id, Socks4Packet.PacketType.DATA, data);
                frame = new C2Frame(DroneId, FrameType.SOCKS_PROXY, await _crypto.Encrypt(packet));
                _tasks.CacheFrame(frame);
                
                // wait for response
                byte[] response;
                while (!_dataQueue.TryDequeue(out response))
                {
                    await Task.Delay(100);
                    
                    if (_tokenSource.IsCancellationRequested)
                        break;
                }
            
                await stream.WriteStream(response);
            }

            await Task.Delay(100);
        }
        
        // send a disconnect
        packet = new Socks4Packet(Id, Socks4Packet.PacketType.DISCONNECT);
        frame = new C2Frame(DroneId, FrameType.SOCKS_PROXY, await _crypto.Encrypt(packet));
        _tasks.CacheFrame(frame);
        
        client.Dispose();
    }

    private async Task<Socks4ConnectRequest> ReadConnectRequest(Stream stream)
    {
        var data = await stream.ReadStream();
        return new Socks4ConnectRequest(data) { Id = Id };
    }
    
    private static async Task SendConnectReply(Stream stream, bool success)
    {
        var reply = new byte[]
        {
            0x00,
            success ? (byte)0x5a : (byte)0x5b,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        await stream.WriteStream(reply);
    }

    public void Stop()
    {
        _tokenSource.Cancel();
    }

    public static implicit operator SocksProxy(SocksRequest request)
    {
        return new SocksProxy
        {
            Id = Helpers.GenerateShortGuid(),
            DroneId = request.DroneId,
            BindPort = request.BindPort
        };
    }

    public static implicit operator SocksResponse(SocksProxy socksProxy)
    {
        return new SocksResponse
        {
            Id = socksProxy.Id,
            DroneId = socksProxy.DroneId,
            BindPort = socksProxy.BindPort
        };
    }
}