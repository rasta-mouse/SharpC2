using SharpC2.API.Responses;

namespace Client.Models.Handlers;

public sealed class ExtHandler : Handler
{
    public int BindPort { get; set; }

    public static implicit operator ExtHandler(ExtHandlerResponse response)
    {
        if (response is null)
            return null;

        return new ExtHandler
        {
            Id = response.Id,
            Name = response.Name,
            BindPort = response.BindPort
        };
    }
}