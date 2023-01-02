using SharpC2.API.Responses;

namespace Client.Models.Handlers;

public sealed class TcpHandler : Handler
{
    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }

    public string BindAddress
        => Loopback ? "127.0.0.1" : "0.0.0.0";

    public static implicit operator TcpHandler(TcpHandlerResponse response)
    {
        if (response is null)
            return null;

        return new TcpHandler
        {
            Id = response.Id,
            Name = response.Name,
            Address = response.Address,
            Port = response.Port,
            Loopback = response.Loopback
        };
    }
}