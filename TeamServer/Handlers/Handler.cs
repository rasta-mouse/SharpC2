namespace TeamServer.Handlers;

public abstract class Handler
{
    public string Name { get; protected set; }
    public PayloadType PayloadType { get; protected set; }
    
    public abstract HandlerType HandlerType { get; }
}

public enum HandlerType
{
    HTTP,
    SMB,
    TCP,
    EXTERNAL
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