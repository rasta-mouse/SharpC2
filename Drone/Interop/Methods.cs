using System;
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
    public static IntPtr OpenProcessToken(IntPtr hProcess, TOKEN_ACCESS tokenAccess)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { hProcess, tokenAccess, hToken };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "OpenProcessToken",
            typeof(OpenProcessToken),
            ref parameters);

        hToken = (IntPtr)parameters[2];
        return hToken;
    }
    
    public static IntPtr LogonUserW(string username, string domain, string password, LOGON_USER_TYPE logonType,
        LOGON_USER_PROVIDER logonProvider)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { username, domain, password, logonType,logonProvider, hToken };
        
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "LogonUserW",
            typeof(LogonUserW),
            ref parameters);
        
        hToken = (IntPtr)parameters[5];
        return hToken;
    }
    
    public static bool ImpersonateToken(IntPtr hToken)
    {
        object[] parameters = { hToken };
        
        return (bool)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "ImpersonateLoggedOnUser",
            typeof(ImpersonateLoggedOnUser),
            ref parameters);
    }
    
    public static IntPtr DuplicateTokenEx(IntPtr hExistingToken, DInvoke.Data.Win32.WinNT.ACCESS_MASK tokenAccess,
        SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType)
    {
        var hNewToken = IntPtr.Zero;
        
        var lpTokenAttributes = new SECURITY_ATTRIBUTES();
        lpTokenAttributes.nLength = Marshal.SizeOf(lpTokenAttributes);

        object[] parameters =
        {
            hExistingToken, (uint)tokenAccess, lpTokenAttributes, impersonationLevel,
            tokenType, hNewToken
        };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "DuplicateTokenEx",
            typeof(DuplicateTokenEx),
            ref parameters);

        hNewToken = (IntPtr)parameters[5];
        return hNewToken;
    }

    public static bool CreateProcessWithToken(IntPtr hToken, LOGON_FLAGS logonFlags, string commandLine, PROCESS_CREATION_FLAGS creationFlags, ref STARTUPINFOEX startupInfo, out PROCESS_INFORMATION processInfo)
    {
        var pi = new PROCESS_INFORMATION();
        
        object[] parameters =
        {
            hToken, logonFlags, commandLine, "", creationFlags, IntPtr.Zero,
            Directory.GetCurrentDirectory(), startupInfo, pi
        };
        
        var success = (bool)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "CreateProcessWithTokenW",
            typeof(CreateProcessWithTokenW),
            ref parameters);

        processInfo = (PROCESS_INFORMATION)parameters[8];
        return success;
    }
    
    public static bool RevertToSelf()
    {
        object[] parameters = { };

        return (bool) Generic.DynamicApiInvoke(
            "advapi32.dll",
            "RevertToSelf",
            typeof(RevertToSelf),
            ref parameters);
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
    
    public static Native.NTSTATUS NtOpenProcess(uint pid, uint desiredAccess, ref IntPtr hProcess)
    {
        var oa = new OBJECT_ATTRIBUTES();
        var ci = new CLIENT_ID { UniqueProcess = (IntPtr)pid };

        object[] parameters = { hProcess, desiredAccess, oa, ci };

        var status = (Native.NTSTATUS)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtOpenProcess",
            typeof(NtOpenProcess),
            ref parameters);

        hProcess = (IntPtr)parameters[0];
        return status;
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

    public static bool NtQueryInformationProcessWow64Information(IntPtr hProcess)
    {
        var result = NtQueryInformationProcess(
            hProcess,
            PROCESS_INFO_CLASS.ProcessWow64Information,
            out var pProcInfo);

        if (result != 0)
            throw new UnauthorizedAccessException("Access is denied.");

        return Marshal.ReadIntPtr(pProcInfo) != IntPtr.Zero;
    }

    public static PROCESS_BASIC_INFORMATION QueryProcessBasicInformation(IntPtr hProcess)
    {
        NtQueryInformationProcess(
            hProcess,
            PROCESS_INFO_CLASS.ProcessBasicInformation,
            out var pProcInfo);

        return (PROCESS_BASIC_INFORMATION)Marshal.PtrToStructure(pProcInfo, typeof(PROCESS_BASIC_INFORMATION));
    }
    
    public static uint NtQueryInformationProcess(IntPtr hProcess, PROCESS_INFO_CLASS processInfoClass, out IntPtr pProcInfo)
    {
        int processInformationLength;
        uint retLen = 0;

        switch (processInfoClass)
        {
            case PROCESS_INFO_CLASS.ProcessWow64Information:
                pProcInfo = Marshal.AllocHGlobal(IntPtr.Size);
                RtlZeroMemory(pProcInfo, IntPtr.Size);
                processInformationLength = IntPtr.Size;
                break;

            case PROCESS_INFO_CLASS.ProcessBasicInformation:
                var pbi = new PROCESS_BASIC_INFORMATION();
                pProcInfo = Marshal.AllocHGlobal(Marshal.SizeOf(pbi));
                RtlZeroMemory(pProcInfo, Marshal.SizeOf(pbi));
                Marshal.StructureToPtr(pbi, pProcInfo, true);
                processInformationLength = Marshal.SizeOf(pbi);
                break;

            default:
                throw new InvalidOperationException($"Invalid ProcessInfoClass: {processInfoClass}");
        }

        object[] parameters = { hProcess, processInfoClass, pProcInfo, processInformationLength, retLen };

        var status = (uint)Generic.DynamicApiInvoke(
            "ntdll.dll",
            "NtQueryInformationProcess",
            typeof(Delegates.NtQueryInformationProcess),
            ref parameters);

        pProcInfo = (IntPtr)parameters[2];
        return status;
    }
    
    public static void RtlZeroMemory(IntPtr destination, int length)
    {
        object[] parameters = { destination, length };

        Generic.DynamicApiInvoke(
            "ntdll.dll",
            "RtlZeroMemory",
            typeof(RtlZeroMemory),
            ref parameters);
    }
}