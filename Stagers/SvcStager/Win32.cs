using System;
using System.Runtime.InteropServices;

using DInvoke.DynamicInvoke;

namespace Drone;

public static class Win32
{
    public const uint INFINITE = 0xFFFFFFFF;
    private const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;

    public static void WaitForObject(IntPtr hObject, uint milliseconds)
    {
        object[] parameters = { hObject, milliseconds };
        
        Generic.DynamicApiInvoke(
            "kernel32.dll",
            "WaitForSingleObject",
            typeof(WaitForSingleObject),
            ref parameters);
    }

    public static IntPtr LoadLibraryEx(string name)
    {
        object[] parameters = { name, IntPtr.Zero, DONT_RESOLVE_DLL_REFERENCES };

        return (IntPtr)Generic.DynamicApiInvoke(
            "kernel32.dll",
            "LoadLibraryW",
            typeof(LoadLibraryExW),
            ref parameters);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate IntPtr LoadLibraryExW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName,
        IntPtr hFile,
        uint dwFlags);
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint WaitForSingleObject(
        IntPtr hObject,
        uint dwMilliseconds);
    
    [Flags]
    public enum ACCESS_MASK : uint
    {
        DELETE = 0x00010000,
        READ_CONTROL = 0x00020000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        SYNCHRONIZE = 0x00100000,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000,
        STANDARD_RIGHTS_READ = 0x00020000,
        STANDARD_RIGHTS_WRITE = 0x00020000,
        STANDARD_RIGHTS_EXECUTE = 0x00020000,
        STANDARD_RIGHTS_ALL = 0x001F0000,
        SPECIFIC_RIGHTS_ALL = 0x0000FFFF,
        ACCESS_SYSTEM_SECURITY = 0x01000000,
        MAXIMUM_ALLOWED = 0x02000000,
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000,
        DESKTOP_READOBJECTS = 0x00000001,
        DESKTOP_CREATEWINDOW = 0x00000002,
        DESKTOP_CREATEMENU = 0x00000004,
        DESKTOP_HOOKCONTROL = 0x00000008,
        DESKTOP_JOURNALRECORD = 0x00000010,
        DESKTOP_JOURNALPLAYBACK = 0x00000020,
        DESKTOP_ENUMERATE = 0x00000040,
        DESKTOP_WRITEOBJECTS = 0x00000080,
        DESKTOP_SWITCHDESKTOP = 0x00000100,
        WINSTA_ENUMDESKTOPS = 0x00000001,
        WINSTA_READATTRIBUTES = 0x00000002,
        WINSTA_ACCESSCLIPBOARD = 0x00000004,
        WINSTA_CREATEDESKTOP = 0x00000008,
        WINSTA_WRITEATTRIBUTES = 0x00000010,
        WINSTA_ACCESSGLOBALATOMS = 0x00000020,
        WINSTA_EXITWINDOWS = 0x00000040,
        WINSTA_ENUMERATE = 0x00000100,
        WINSTA_READSCREEN = 0x00000200,
        WINSTA_ALL_ACCESS = 0x0000037F
    }
    
    public enum MEMORY_PROTECTION : uint
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