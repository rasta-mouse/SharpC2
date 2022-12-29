namespace SharpC2.API.Responses;

public sealed class ReversePortForwardResponse
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }
    public string ForwardHost { get; set; }
    public int ForwardPort { get; set; }
}