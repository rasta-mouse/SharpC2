using TeamServer.Utilities;

namespace TeamServer.Handlers;

public sealed class TcpHandler : Handler
{
    public override HandlerType HandlerType
        => HandlerType.TCP;

    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }

    public TcpHandler(bool reverse)
    {
        PayloadType = reverse
            ? PayloadType.REVERSE_TCP
            : PayloadType.BIND_TCP;
        
        Id = Helpers.GenerateShortGuid();
    }

    public TcpHandler()
    {
        PayloadType = PayloadType.BIND_TCP;
        Id = Helpers.GenerateShortGuid();
    }
}