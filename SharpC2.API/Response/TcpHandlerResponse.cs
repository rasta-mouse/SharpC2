namespace SharpC2.API.Response;

public sealed class TcpHandlerResponse
{
    public string Name { get; set; }
    public int BindPort { get; set; }
    public bool LoopbackOnly { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}