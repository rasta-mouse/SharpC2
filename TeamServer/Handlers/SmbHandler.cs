using TeamServer.Utilities;

namespace TeamServer.Handlers;

public sealed class SmbHandler : Handler
{
    public override HandlerType HandlerType
        => HandlerType.SMB;

    public string PipeName { get; set; }

    public SmbHandler()
    {
        Id = Helpers.GenerateShortGuid();
        PayloadType = PayloadType.BIND_PIPE;
    }
}