using System.Runtime.InteropServices;

namespace ExampleController;

public static class Win32
{
    [DllImport("kernel32.dll")]
    public static extern bool PeekNamedPipe(
        IntPtr hNamedPipe,
        IntPtr lpBuffer,
        IntPtr nBufferSize,
        IntPtr lpBytesRead,
        ref uint lpTotalBytesAvail,
        IntPtr lpBytesLeftThisMessage);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAlloc(
        IntPtr lpAddress,
        IntPtr dwSize,
        AllocationType flAllocationType,
        MemoryProtection flProtect);
    
    [DllImport("kernel32")]
    public static extern IntPtr CreateThread(
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        IntPtr lpThreadId);
    
    [Flags]
    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }
}