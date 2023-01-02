using SharpC2.API.Responses;

namespace Client.Models.Handlers;

public sealed class SmbHandler : Handler
{
    public string PipeName { get; set; }

    public static implicit operator SmbHandler(SmbHandlerResponse response)
    {
        if (response is null)
            return null;

        return new SmbHandler
        {
            Id = response.Id,
            Name = response.Name,
            PipeName = response.PipeName
        };
    }
}