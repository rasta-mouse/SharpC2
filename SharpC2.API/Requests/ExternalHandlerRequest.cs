namespace SharpC2.API.Requests;

public sealed class ExternalHandlerRequest
{
    public string Name { get; set; }
    public int BindPort { get; set; }
}