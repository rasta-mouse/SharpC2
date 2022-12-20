namespace Client.Models.Handlers;

public class HttpHandler : Handler
{
    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
}