namespace SharpC2.Models;

public class Handler
{
    public string Name { get; set; }
    public PayloadType PayloadType { get; set; }
    public HandlerType HandlerType { get; set; }

    public override string ToString()
    {
        return $"{Name} [[{HandlerType}]]";
    }
}

public enum HandlerType
{
    HTTP,
    SMB,
    TCP
}

public enum PayloadType
{
    REVERSE_HTTP,
    REVERSE_HTTPS,
    BIND_PIPE,
    BIND_TCP,
    REVERSE_TCP,
    CUSTOM
}