namespace Client.Models.Handlers;

public sealed class TcpHandler : Handler
{
    public string Address { get; set; }
    public int Port { get; set; }
    public bool Loopback { get; set; }

    public string BindAddress
        => Loopback ? "127.0.0.1" : "0.0.0.0";
}