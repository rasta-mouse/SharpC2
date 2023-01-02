using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Utilities;

namespace TeamServer.Handlers;

public sealed class SmbHandler : Handler
{
    public override HandlerType HandlerType
        => HandlerType.SMB;

    public string PipeName { get; set; }

    public static implicit operator SmbHandler(SmbHandlerRequest request)
    {
        return new SmbHandler
        {
            Id = Helpers.GenerateShortGuid(),
            Name = request.Name,
            PipeName = request.PipeName,
            PayloadType = PayloadType.BIND_PIPE
        };
    }

    public static implicit operator SmbHandlerResponse(SmbHandler handler)
    {
        return new SmbHandlerResponse
        {
            Id = handler.Id,
            Name = handler.Name,
            PipeName = handler.PipeName,
            HandlerType = (int)handler.HandlerType,
            PayloadType = (int)handler.PayloadType
        };
    }
}