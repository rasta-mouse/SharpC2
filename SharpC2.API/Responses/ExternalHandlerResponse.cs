namespace SharpC2.API.Responses;

public sealed class ExternalHandlerResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int BindPort { get; set; }
}