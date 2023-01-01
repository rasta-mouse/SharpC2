namespace SharpC2.API.Responses;

public class DroneResponse
{
    public MetadataResponse Metadata { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public string Parent { get; set; }
    public int Status { get; set; }
}

public class MetadataResponse
{
    public string Id { get; set; }
    public string Identity { get; set; }
    public byte[] Address { get; set; }
    public string Hostname { get; set; }
    public string Process { get; set; }
    public int Pid { get; set; }
    public bool Is64Bit { get; set; }
    public int Integrity { get; set; }
}