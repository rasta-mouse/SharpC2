namespace TeamServer.Handlers;

public class SmbHandler : Handler
{
    public string PipeName { get; set; }

    public override HandlerType HandlerType
        => HandlerType.SMB;

    public SmbHandler(string name, string pipeName)
    {
        Name = name;
        PipeName = pipeName;
        
        PayloadType = PayloadType.BIND_PIPE;
    }
}