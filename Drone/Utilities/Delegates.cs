using System;
using System.Runtime.InteropServices;

namespace Drone.Utilities;

public static class Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool InitializeProcThreadAttributeList(
        IntPtr lpAttributeList,
        int dwAttributeCount,
        int dwFlags,
        ref IntPtr lpSize);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool UpdateProcThreadAttribute(
        IntPtr lpAttributeList,
        uint dwFlags,
        IntPtr attribute,
        IntPtr lpValue,
        IntPtr cbSize,
        IntPtr lpPreviousValue,
        IntPtr lpReturnSize);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool CreatePipe(
        out IntPtr hReadPipe,
        out IntPtr hWritePipe,
        ref Win32.SECURITY_ATTRIBUTES lpPipeAttributes, 
        uint nSize);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool SetHandleInformation(
        IntPtr hObject,
        Win32.HANDLE_FLAGS dwMask,
        Win32.HANDLE_FLAGS dwFlags);
    
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
        ref Win32.SECURITY_ATTRIBUTES lpProcessAttributes,
        ref Win32.SECURITY_ATTRIBUTES lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
        ref Win32.STARTUPINFOEX lpStartupInfo,
        out Win32.PROCESS_INFORMATION lpProcessInformation);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate IntPtr QueueUserApc(
        IntPtr pfnAPC,
        IntPtr hThread,
        IntPtr dwData);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool PeekNamedPipe(
        IntPtr hNamedPipe,
        IntPtr lpBuffer,
        IntPtr nBufferSize,
        IntPtr lpBytesRead,
        ref uint lpTotalBytesAvail,
        IntPtr lpBytesLeftThisMessage);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate IntPtr OpenScManagerW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpMachineName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDatabaseName,
        uint dwDesiredAccess);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate IntPtr CreateServiceW(
        IntPtr hScManager,
        [MarshalAs(UnmanagedType.LPWStr)] string lpServiceName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        [MarshalAs(UnmanagedType.LPWStr)] string lpBinaryPathName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpLoadOrderGroup,
        uint lpdwTagId,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDependencies,
        [MarshalAs(UnmanagedType.LPWStr)] string lpServiceStartName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpPassword);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool StartServiceW(
        IntPtr hService,
        uint dwNumServiceArgs,
        string[] lpServiceArgVectors);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DeleteService(IntPtr hService);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CloseHandle(IntPtr hObject);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]
    public delegate bool LogonUserW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszUsername,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszDomain,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszPassword,
        Win32.LOGON_USER_TYPE dwLogonType,
        Win32.LOGON_USER_PROVIDER dwLogonProvider,
        out IntPtr phToken);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool ImpersonateLoggedOnUser(IntPtr hToken);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtOpenProcess(
        ref IntPtr processHandle,
        uint desiredAccess,
        ref Native.OBJECT_ATTRIBUTES objectAttributes,
        ref Native.CLIENT_ID clientId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool OpenProcessToken(
        IntPtr processHandle,
        Win32.TOKEN_ACCESS desiredAccess,
        out IntPtr tokenHandle);

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool DuplicateTokenEx(
        IntPtr hExistingToken,
        uint dwDesiredAccess,
        ref Win32.SECURITY_ATTRIBUTES lpTokenAttributes,
        Win32.SECURITY_IMPERSONATION_LEVEL impersonationLevel,
        Win32.TOKEN_TYPE tokenType,
        out IntPtr phNewToken);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    public delegate bool RevertToSelf();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RtlZeroMemory(
        IntPtr destination,
        int length);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtQueryInformationProcess(
        IntPtr processHandle,
        Native.PROCESS_INFO_CLASS processInformationClass,
        IntPtr processInformation,
        int processInformationLength,
        ref uint returnLength);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtAllocateVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        IntPtr zeroBits,
        ref IntPtr regionSize,
        uint allocationType,
        uint memoryProtection);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtWriteVirtualMemory(
        IntPtr processHandle,
        IntPtr baseAddress,
        IntPtr buffer,
        uint bufferLength,
        ref uint bytesWritten);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtFreeVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        ref IntPtr regionSize,
        uint freeType);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtProtectVirtualMemory(
        IntPtr processHandle,
        ref IntPtr baseAddress,
        ref IntPtr regionSize,
        uint newProtect,
        ref uint oldProtect);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate uint NtCreateThreadEx(
        out IntPtr threadHandle,
        DInvoke.Data.Win32.WinNT.ACCESS_MASK desiredAccess,
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
    public delegate uint NtResumeThread(
        IntPtr hThread,
        ref uint suspendCount);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CreateProcessWithLogonW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpUsername,
        [MarshalAs(UnmanagedType.LPWStr)] string lpDomain,
        [MarshalAs(UnmanagedType.LPWStr)] string lpPassword,
        uint dwLogonFlags,
        [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCommandLine,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        [MarshalAs(UnmanagedType.LPWStr)] string lpCurrentDirectory,
        ref Win32.STARTUPINFOEX lpStartupInfo,
        out Win32.PROCESS_INFORMATION lpProcessInformation);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public delegate uint AmsiScanBuffer(
        IntPtr amsiContext,
        byte[] buffer,
        uint length,
        string contentName,
        IntPtr session,
        out Win32.AMSI_RESULT result);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate ulong EtwEventWrite(
        IntPtr regHandle,
        IntPtr eventDescriptor,
        ulong userDataCount,
        IntPtr userData);
}