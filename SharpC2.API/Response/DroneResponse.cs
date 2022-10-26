namespace SharpC2.API.Response;

public sealed class DroneResponse
{
    public MetadataResponse Metadata { get; set; }
    public string Parent { get; set; }
    public string ExternalAddress { get; set; }
    public string Handler { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public int Status { get; set; }
}