namespace SharpC2.API.Responses;

public sealed class TcpHandlerResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}