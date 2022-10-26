namespace SharpC2.API.Response;

public sealed class SmbHandlerResponse
{
    public string Name { get; set; }
    public string PipeName { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}