namespace SharpC2.API.Requests;

public sealed class TcpHandlerRequest
{
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }
}