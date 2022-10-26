namespace SharpC2.API.Response;

public sealed class HandlerResponse
{
    public string Name { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}