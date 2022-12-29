namespace SharpC2.API.Requests;

public sealed class ReversePortForwardRequest
{
    public string DroneId { get; set; }
    public int BindPort { get; set; }
    public string ForwardHost { get; set; }
    public int ForwardPort { get; set; }
}