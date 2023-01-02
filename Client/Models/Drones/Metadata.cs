using System.Net;
using SharpC2.API.Responses;

namespace Client.Models.Drones;

public class Metadata
{
    public string Id { get; set; }
    public string Identity { get; set; }
    public byte[] Address { get; set; }
    public string Hostname { get; set; }
    public string Process { get; set; }
    public int Pid { get; set; }
    public bool Is64Bit { get; set; }
    public IntegrityLevel Integrity { get; set; }

    public string IPAddress
        => new IPAddress(Address).ToString();

    public string Arch
        => Is64Bit ? "x64" : "x86";

    public string IntegrityLevel
        => Integrity.ToString();

    public static implicit operator Metadata(MetadataResponse response)
    {
        if (response is null)
            return null;

        return new Metadata
        {
            Id = response.Id,
            Identity = response.Identity,
            Address = response.Address,
            Hostname = response.Hostname,
            Process = response.Process,
            Pid = response.Pid,
            Is64Bit = response.Is64Bit,
            Integrity = (IntegrityLevel)response.Integrity
        };
    }
}

public enum IntegrityLevel
{
    Medium,
    High,
    SYSTEM
}