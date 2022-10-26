using System;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;

namespace Drone;

public static class Native
{
    public static bool AllocateVirtualMemory(IntPtr hProcess, int size, ref IntPtr baseAddress)
    {
        var regionSize = new IntPtr(size);
        const MEMORY_ALLOCATION allocation = MEMORY_ALLOCATION.MEM_COMMIT | MEMORY_ALLOCATION.MEM_RESERVE;
        
        object[] parameters =
        {
            hProcess, baseAddress, IntPtr.Zero, regionSize,
            (uint)allocation, MEMORY_PROTECTION.PAGE_READWRITE
        };
        
        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtAllocateVirtualMemory",
            typeof(NtAllocateVirtualMemory),
            ref parameters);
        
        baseAddress = (IntPtr)parameters[1];
        return status == 0;
    }

    public static bool WriteVirtualMemory(IntPtr hProcess, IntPtr baseAddress, byte[] data)
    {
        var buf = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, buf, data.Length);
        
        uint bytesWritten = 0;
        
        object[] parameters = 
        {
            hProcess, baseAddress, buf, (uint)data.Length, bytesWritten
        };
        
        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtWriteVirtualMemory",
            typeof(NtWriteVirtualMemory),
            ref parameters);
        
        Marshal.FreeHGlobal(buf);
        
        bytesWritten = (uint)parameters[4];
        return status == 0 && bytesWritten > 0;
    }
    
    public static bool ProtectVirtualMemory(IntPtr hProcess, IntPtr baseAddress, int size, uint protection)
    {
        var regionSize = new IntPtr(size);
        uint oldProtect = 0;
        
        object[] parameters = 
        {
            hProcess, baseAddress, regionSize, protection, oldProtect
        };
        
        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtProtectVirtualMemory",
            typeof(NtProtectVirtualMemory),
            ref parameters);

        oldProtect = (uint)parameters[4];
        return status == 0;
    }

    public static bool CreateThreadEx(IntPtr hProcess, IntPtr address, IntPtr parameter, ref IntPtr hThread)
    {
        object[] parameters =
        {
            hThread, (uint)Win32.ACCESS_MASK.GENERIC_ALL, IntPtr.Zero, hProcess, address, parameter,
            false, 0, 0, 0, IntPtr.Zero
        };

        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtCreateThreadEx",
            typeof(NtCreateThreadEx),
            ref parameters);

        hThread = (IntPtr)parameters[0];
        return status == 0;
    }

    public static bool FreeVirtualMemory(IntPtr hProcess, IntPtr baseAddress, int size)
    {
        var regionSize = new IntPtr(size);
        
        object[] parameters =
        {
            hProcess, baseAddress, regionSize, (uint)MEMORY_ALLOCATION.MEM_RELEASE
        };
        
        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtFreeVirtualMemory",
            typeof(NtFreeVirtualMemory),
            ref parameters);

        return status == 0;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint NtAllocateVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        IntPtr zeroBits,
        ref IntPtr regionSize,
        uint allocationType,
        uint memoryProtection);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint NtWriteVirtualMemory(
        IntPtr processHandle,
        IntPtr baseAddress,
        IntPtr buffer,
        uint bufferLength,
        ref uint bytesWritten);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint NtProtectVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        ref IntPtr regionSize,
        uint newProtect,
        ref uint oldProtect);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint NtCreateThreadEx(
        out IntPtr threadHandle,
        uint desiredAccess,
        IntPtr objectAttributes,
        IntPtr processHandle,
        IntPtr startAddress,
        IntPtr parameter,
        bool createSuspended,
        int stackZeroBits,
        int sizeOfStack,
        int maximumStackSize,
        IntPtr attributeList);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtFreeVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        ref IntPtr regionSize,
        uint freeType);
    
    [Flags]
    private enum MEMORY_ALLOCATION : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_RESET = 0x80000,
        MEM_RESET_UNDO = 0x1000000,
        MEM_LARGE_PAGES = 0x20000000,
        MEM_PHYSICAL = 0x400000,
        MEM_TOP_DOWN = 0x100000,
        MEM_WRITE_WATCH = 0x200000,
        MEM_COALESCE_PLACEHOLDERS = 0x1,
        MEM_PRESERVE_PLACEHOLDER = 0x2,
        MEM_DECOMMIT = 0x4000,
        MEM_RELEASE = 0x8000
    }
    
    private enum MEMORY_PROTECTION : uint
    {
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000
    }
}