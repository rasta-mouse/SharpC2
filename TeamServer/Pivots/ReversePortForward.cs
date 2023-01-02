using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Utilities;

namespace TeamServer.Pivots;

public sealed class ReversePortForward
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }
    public string ForwardHost { get; set; }
    public int ForwardPort { get; set; }

    public static implicit operator ReversePortForward(ReversePortForwardRequest request)
    {
        return new ReversePortForward
        {
            Id = Helpers.GenerateShortGuid(),
            DroneId = request.DroneId,
            BindPort = request.BindPort,
            ForwardHost = request.ForwardHost,
            ForwardPort = request.ForwardPort
        };
    }

    public static implicit operator ReversePortForwardResponse(ReversePortForward fwd)
    {
        return new ReversePortForwardResponse
        {
            Id = fwd.Id,
            DroneId = fwd.DroneId,
            BindPort = fwd.BindPort,
            ForwardHost = fwd.ForwardHost,
            ForwardPort = fwd.ForwardPort
        };
    }
}