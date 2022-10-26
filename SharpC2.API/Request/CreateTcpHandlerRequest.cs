namespace SharpC2.API.Request;

public sealed class CreateTcpHandlerRequest
{
    public string Name { get; set; }
    public int BindPort { get; set; }
    public bool LoopbackOnly { get; set; }
}