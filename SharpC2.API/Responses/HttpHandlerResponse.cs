namespace SharpC2.API.Responses;

public sealed class HttpHandlerResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public bool Secure { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}