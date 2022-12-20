using System;
using System.Runtime.InteropServices;
using DInvoke.Data;

namespace Drone.Interop;

using static Data;

public static class Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate bool LogonUserW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszUsername,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszDomain,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszPassword,
        LOGON_USER_TYPE dwLogonType,
        LOGON_USER_PROVIDER dwLogonProvider,
        out IntPtr phToken);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool ImpersonateLoggedOnUser(IntPtr hToken);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DuplicateTokenEx(
        IntPtr hExistingToken,
        uint dwDesiredAccess,
        ref SECURITY_ATTRIBUTES lpTokenAttributes,
        SECURITY_IMPERSONATION_LEVEL impersonationLevel,
        TOKEN_TYPE tokenType,
        out IntPtr phNewToken);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool RevertToSelf();
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DuplicateHandle(
        IntPtr hSourceProcessHandle,
        IntPtr hSourceHandle, 
        IntPtr hTargetProcessHandle,
        ref IntPtr lpTargetHandle,
        uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwOptions);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate bool CreateProcessW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
        ref SECURITY_ATTRIBUTES lpProcessAttributes,
        ref SECURITY_ATTRIBUTES lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
        ref STARTUPINFOEX lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate bool CreateProcessWithTokenW(
        IntPtr hToken,
        LOGON_FLAGS dwLogonFlags,
        [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
        PROCESS_CREATION_FLAGS dwCreationFlags,
        IntPtr lpEnvironment,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
        ref STARTUPINFOEX lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool PeekNamedPipe(
        IntPtr hNamedPipe,
        IntPtr lpBuffer,
        IntPtr nBufferSize,
        IntPtr lpBytesRead,
        ref uint lpTotalBytesAvail,
        IntPtr lpBytesLeftThisMessage);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CloseHandle(IntPtr hObject);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool OpenProcessToken(
        IntPtr processHandle,
        TOKEN_ACCESS desiredAccess,
        out IntPtr tokenHandle);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate Native.NTSTATUS NtOpenProcess(
        ref IntPtr processHandle,
        PROCESS_ACCESS_FLAGS desiredAccess,
        ref OBJECT_ATTRIBUTES objectAttributes,
        ref CLIENT_ID clientId);
    
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
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RtlZeroMemory(
        IntPtr destination,
        int length);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueryInformationProcess(
        IntPtr processHandle,
        PROCESS_INFO_CLASS processInformationClass,
        IntPtr processInformation,
        int processInformationLength,
        ref uint returnLength);
}