using SharpC2.API.Requests;
using SharpC2.API.Responses;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public sealed class TcpHandler : Handler
{
    public override HandlerType HandlerType
        => HandlerType.TCP;

    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }

    public static implicit operator TcpHandler(TcpHandlerRequest request)
    {
        return new TcpHandler
        {
            Id = Helpers.GenerateShortGuid(),
            Name = request.Name,
            Address = request.Address,
            Port = request.Port,
            Loopback = request.Loopback,
            PayloadType = string.IsNullOrWhiteSpace(request.Address) ? PayloadType.BIND_TCP : PayloadType.REVERSE_TCP
        };
    }

    public static implicit operator TcpHandlerResponse(TcpHandler handler)
    {
        return new TcpHandlerResponse
        {
            Id = handler.Id,
            Name = handler.Name,
            Address = handler.Address,
            Port = handler.Port,
            Loopback = handler.Loopback,
            HandlerType = (int)handler.HandlerType,
            PayloadType = (int)handler.PayloadType
        };
    }
}