namespace SharpC2.API.Responses;

public sealed class SmbHandlerResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string PipeName { get; set; }
    public int PayloadType { get; set; }
    public int HandlerType { get; set; }
}