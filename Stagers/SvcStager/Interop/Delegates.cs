using System;
using System.Runtime.InteropServices;

using DInvoke.Data;

namespace Drone.Interop;

using static Data;

public static class Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool CreateProcessW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
        ref SECURITY_ATTRIBUTES lpProcessAttributes,
        ref SECURITY_ATTRIBUTES lpThreadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
        PROCESS_CREATION_FLAGS dwCreationFlags,
        IntPtr lpEnvironment,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CloseHandle(IntPtr hObject);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate Native.NTSTATUS NtAllocateVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        IntPtr zeroBits,
        ref IntPtr regionSize,
        MEMORY_ALLOCATION allocationType,
        MEMORY_PROTECTION memoryProtection);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate Native.NTSTATUS NtWriteVirtualMemory(
        IntPtr processHandle,
        IntPtr baseAddress,
        IntPtr buffer,
        uint bufferLength,
        ref uint bytesWritten);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate Native.NTSTATUS NtProtectVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        ref IntPtr regionSize,
        MEMORY_PROTECTION newProtect,
        ref MEMORY_PROTECTION oldProtect);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate Native.NTSTATUS NtCreateThreadEx(
        out IntPtr threadHandle,
        Win32.WinNT.ACCESS_MASK desiredAccess,
        IntPtr objectAttributes,
        IntPtr processHandle,
        IntPtr startAddress,
        IntPtr parameter,
        bool createSuspended,
        int stackZeroBits,
        int sizeOfStack,
        int maximumStackSize,
        IntPtr attributeList);
}