using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;

namespace Drone.Utilities;

public static class Win32
{
    public const uint SC_MANAGER_ALL_ACCESS = 0xF003F;
    public const uint SERVICE_ALL_ACCESS = 0xF01FF;
    public const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
    public const uint SERVICE_DEMAND_START = 0x00000003;
    public const uint SERVICE_ERROR_IGNORE = 0x00000000;
    
    public const uint DUPLICATE_CLOSE_SOURCE = 0x00000001;
    public const uint DUPLICATE_SAME_ACCESS = 0x00000002;

    public const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
    public const int PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;
    
    public const int STARTF_USESTDHANDLES = 0x00000100;
    public const int STARTF_USESHOWWINDOW = 0x00000001;
    public const short SW_HIDE = 0x0000;
    
    public const long BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000;

    public static bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, ref IntPtr lpSize)
    {
        object[] parameters = { lpAttributeList, dwAttributeCount, 0, lpSize };
        
        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "InitializeProcThreadAttributeList",
            typeof(Delegates.InitializeProcThreadAttributeList),
            ref parameters);

        lpSize = (IntPtr)parameters[3];
        return success;
    }

    public static bool UpdateProcThreadAttribute(IntPtr lpAttributeList, IntPtr attribute, IntPtr lpValue, IntPtr cbSize)
    {
        object[] parameters = { lpAttributeList, (uint)0, attribute, lpValue, cbSize, IntPtr.Zero, IntPtr.Zero };
        
        return (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "UpdateProcThreadAttribute",
            typeof(Delegates.UpdateProcThreadAttribute),
            ref parameters);
    }

    public static void DeleteProcThreadAttributeList(IntPtr lpAttributeList)
    {
        object[] parameters = { lpAttributeList };

        Generic.DynamicApiInvoke(
            "kernel32.dll",
            "DeleteProcThreadAttributeList",
            typeof(Delegates.DeleteProcThreadAttributeList),
            ref parameters);
    }

    public static bool CreatePipe(out IntPtr hReadPipe, out IntPtr hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes,
        uint nSize)
    {
        object[] parameters = { IntPtr.Zero, IntPtr.Zero, lpPipeAttributes, nSize };

        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CreatePipe",
            typeof(Delegates.CreatePipe),
            ref parameters);

        hReadPipe = (IntPtr)parameters[0];
        hWritePipe = (IntPtr)parameters[1];

        return success;
    }

    public static bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags)
    {
        object[] parameters = { hObject, dwMask, dwFlags };

        return (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "SetHandleInformation",
            typeof(Delegates.SetHandleInformation),
            ref parameters);
    }

    public static bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle,
        ref IntPtr lpTargetHandle, uint dwDesiredAccess, bool bInheritHandle, uint dwOptions)
    {
        object[] parameters =
        {
            hSourceProcessHandle, hSourceHandle, hTargetProcessHandle,
            lpTargetHandle, dwDesiredAccess, bInheritHandle, dwOptions
        };

        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "DuplicateHandle",
            typeof(Delegates.DuplicateHandle),
            ref parameters);

        lpTargetHandle = (IntPtr)parameters[3];
        return success;
    }

    public static uint WaitForSingleObject(IntPtr handle, uint milliseconds)
    {
        object[] parameters = { handle, milliseconds };

        return (uint)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "WaitForSingleObject",
            typeof(Delegates.WaitForSingleObject),
            ref parameters);
    }

    public static IntPtr LogonUserW(string username, string domain, string password, LOGON_USER_TYPE logonType,
        LOGON_USER_PROVIDER logonProvider)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { username, domain, password, logonType,logonProvider, hToken };
        
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "LogonUserW",
            typeof(Delegates.LogonUserW),
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
            typeof(Delegates.ImpersonateLoggedOnUser),
            ref parameters);
    }

    public static IntPtr OpenProcessToken(IntPtr hProcess, TOKEN_ACCESS tokenAccess)
    {
        var hToken = IntPtr.Zero;
        object[] parameters = { hProcess, tokenAccess, hToken };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "OpenProcessToken",
            typeof(Delegates.OpenProcessToken),
            ref parameters);

        hToken = (IntPtr)parameters[2];
        return hToken;
    }
    
    public static IntPtr DuplicateTokenEx(IntPtr hExistingToken, DInvoke.Data.Win32.WinNT.ACCESS_MASK tokenAccess,
        SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType)
    {
        var hNewToken = IntPtr.Zero;
        
        var lpTokenAttributes = new SECURITY_ATTRIBUTES();
        //lpTokenAttributes.nLength = Marshal.SizeOf(lpTokenAttributes);

        object[] parameters =
        {
            hExistingToken, (uint)tokenAccess, lpTokenAttributes, impersonationLevel,
            tokenType, hNewToken
        };

        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "DuplicateTokenEx",
            typeof(Delegates.DuplicateTokenEx),
            ref parameters);

        hNewToken = (IntPtr)parameters[5];
        return hNewToken;
    }
    
    public static bool RevertToSelf()
    {
        object[] parameters = { };

        return (bool) Generic.DynamicApiInvoke(
            "advapi32.dll",
            "RevertToSelf",
            typeof(Delegates.RevertToSelf),
            ref parameters);
    }

    public static bool PeekNamedPipe(IntPtr hNamedPipe, ref uint totalBytesAvailable)
    {
        var parameters = new object[]
        {
            hNamedPipe, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, totalBytesAvailable, IntPtr.Zero
        };
                
        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "PeekNamedPipe",
            typeof(Delegates.PeekNamedPipe),
            ref parameters);

        totalBytesAvailable = (uint)parameters[4];
        return success;
    }

    public static bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
        ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, PROCESS_CREATION_FLAGS dwCreationFlags, IntPtr lpEnvironment,
        string lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation)
    {
        var pi = new PROCESS_INFORMATION();
        
        object[] parameters =
        {
            lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles,
            (uint)dwCreationFlags, lpEnvironment, lpCurrentDirectory, lpStartupInfo, pi
        };
        
        var success = (bool)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CreateProcessW",
            typeof(Delegates.CreateProcessW),
            ref parameters);

        lpProcessInformation = (PROCESS_INFORMATION)parameters[9];
        return success;
    }

    public static bool QueueUserApc(IntPtr baseAddress, IntPtr hThread)
    {
        object[] parameters =
        {
            baseAddress, hThread, IntPtr.Zero
        };
        
        var ret = (IntPtr)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "QueueUserAPC",
            typeof(Delegates.QueueUserApc),
            ref parameters);

        return ret != IntPtr.Zero;
    }

    public static void CloseServiceHandle(IntPtr hObject)
    {
        object[] parameters = { hObject };
        
        Generic.DynamicApiInvoke(
            "advapi32.dll",
            "CloseServiceHandle",
            typeof(Delegates.CloseHandle),
            ref parameters);
    }
    
    public static void CloseHandle(IntPtr hObject)
    {
        object[] parameters = { hObject };
        
        Generic.DynamicApiInvoke(
            "kernel32.dll",
            "CloseHandle",
            typeof(Delegates.CloseHandle),
            ref parameters);
    }

    public static bool CreateProcessWithLogonW(string domain, string username, string password,
        string lpCommandLine, bool suspended, out PROCESS_INFORMATION pi)
    {
        var si = new STARTUPINFOEX();
        si.StartupInfo.cb = (uint)Marshal.SizeOf(si);

        pi = new PROCESS_INFORMATION();

        PROCESS_CREATION_FLAGS flags = 0;

        if (suspended)
            flags |= PROCESS_CREATION_FLAGS.CREATE_SUSPENDED;

        object[] parameters =
        {
            username, domain, password, (uint)LOGON_FLAGS.LOGON_WITH_PROFILE,
            null, lpCommandLine, (uint)flags, IntPtr.Zero, Directory.GetCurrentDirectory(), si, pi
        };
        
        var success = (bool)Generic.DynamicApiInvoke(
            "advapi32.dll",
            "CreateProcessWithLogonW",
            typeof(Delegates.CreateProcessWithLogonW),
            ref parameters);

        pi = (PROCESS_INFORMATION)parameters[10];
        return success;
    }
    
    [Flags]
    public enum HANDLE_FLAGS : uint
    {
        None = 0,
        INHERIT = 1,
        PROTECT_FROM_CLOSE = 2
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFO
    {
        public uint cb;
        public IntPtr lpReserved;
        public IntPtr lpDesktop;
        public IntPtr lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct STARTUPINFOEX
    {
        public STARTUPINFO StartupInfo;
        public IntPtr lpAttributeList;
    }

    [Flags]
    public enum PROCESS_CREATION_FLAGS
    {
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SECURE_PROCESS = 0x00400000,
        CREATE_SEPARATE_WOW_VDM = 0x00000800,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_SUSPENDED = 0x00000004,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        DEBUG_PROCESS = 0x00000001,
        DETACHED_PROCESS = 0x00000008,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    public enum LOGON_FLAGS : uint
    {
        LOGON_WITH_PROFILE = 0x00000001,
        LOGON_NETCREDENTIALS_ONLY = 0x00000002
    }

    public enum LOGON_USER_TYPE
    {
        LOGON32_LOGON_INTERACTIVE = 2,
        LOGON32_LOGON_NETWORK = 3,
        LOGON32_LOGON_BATCH = 4,
        LOGON32_LOGON_SERVICE = 5,
        LOGON32_LOGON_UNLOCK = 7,
        LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
        LOGON32_LOGON_NEW_CREDENTIALS = 9
    }
        
    public enum LOGON_USER_PROVIDER
    {
        LOGON32_PROVIDER_DEFAULT = 0,
        LOGON32_PROVIDER_WINNT35 = 1,
        LOGON32_PROVIDER_WINNT40 = 2,
        LOGON32_PROVIDER_WINNT50 = 3,
        LOGON32_PROVIDER_VIRTUAL = 4
    }
    
    [Flags]
    public enum PROCESS_ACCESS_FLAGS : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FFF,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        SYNCHRONIZE = 0x00100000
    }
    
    [Flags]
    public enum DUPLICATE_OPTIONS : uint
    {
        CLOSE_SOURCE = 0x00000001,
        SAME_ACCESS = 0x00000002
    }
    
    [Flags]
    public enum TOKEN_ACCESS : uint
    {
        TOKEN_ASSIGN_PRIMARY = 0x0001,
        TOKEN_DUPLICATE = 0x0002,
        TOKEN_IMPERSONATE = 0x0004,
        TOKEN_QUERY = 0x0008,
        TOKEN_QUERY_SOURCE = 0x0010,
        TOKEN_ADJUST_PRIVILEGES = 0x0020,
        TOKEN_ADJUST_GROUPS = 0x0040,
        TOKEN_ADJUST_DEFAULT = 0x0080,
        TOKEN_ADJUST_SESSIONID = 0x0100,
        TOKEN_ALL_ACCESS_P = 0x000F00FF,
        TOKEN_ALL_ACCESS = 0x000F01FF,
        TOKEN_READ = 0x00020008,
        TOKEN_WRITE = 0x000200E0,
        TOKEN_EXECUTE = 0x00020000
    }
    
    public enum SECURITY_IMPERSONATION_LEVEL
    {
        SECURITY_ANONYMOUS,
        SECURITY_IDENTIFICATION,
        SECURITY_IMPERSONATION,
        SECURITY_DELEGATION
    }
    
    public enum TOKEN_TYPE
    {
        TOKEN_PRIMARY = 1,
        TOKEN_IMPERSONATION = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }
    
    public enum AMSI_RESULT
    {
        AMSI_RESULT_CLEAN = 0,
        AMSI_RESULT_NOT_DETECTED = 1,
        AMSI_RESULT_BLOCKED_BY_ADMIN_START = 16384,
        AMSI_RESULT_BLOCKED_BY_ADMIN_END = 20479,
        AMSI_RESULT_DETECTED = 32768
    }
}