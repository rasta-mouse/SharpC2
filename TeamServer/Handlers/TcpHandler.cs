namespace TeamServer.Handlers;

public class TcpHandler : Handler
{
    public int BindPort { get; set; }
    public bool LoopbackOnly { get; }

    public override HandlerType HandlerType
        => HandlerType.TCP;

    public TcpHandler(string name, int bindPort, bool loopbackOnly)
    {
        Name = name;
        BindPort = bindPort;
        LoopbackOnly = loopbackOnly;

        PayloadType = PayloadType.BIND_TCP;
    }
}