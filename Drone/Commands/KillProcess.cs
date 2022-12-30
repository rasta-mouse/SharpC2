using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public class KillProcess : DroneCommand
{
    public override byte Command => 0x1F;
    public override bool Threaded => false;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var pid = task.Arguments[0];
        
        using var process = Process.GetProcessById(int.Parse(pid));
        process.Kill();
        
        await Drone.SendTaskOutput(task.Id, $"Killed PID {pid} ({process.ProcessName}).");
    }
}