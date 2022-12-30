using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

using static Interop.Methods;

public sealed class RevToSelf : DroneCommand
{
    public override byte Command => 0x2C;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (!RevertToSelf())
            throw new Win32Exception(Marshal.GetLastWin32Error());

        Drone.ImpersonationToken = IntPtr.Zero;
        await Drone.SendTaskComplete(task.Id);
    }
}