namespace SharpC2.API.Request;

public sealed class CreateHttpHandlerRequest
{
    public string Name { get; set; }
    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public bool Secure { get; set; }
}