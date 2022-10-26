namespace SharpC2.Models;

public sealed class TcpHandler : Handler
{
    public int BindPort { get; set; }
    public bool LoopbackOnly { get; set; }
}