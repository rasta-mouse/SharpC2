using SharpC2.API.Responses;

namespace Client.Models.Pivots;

public sealed class ReversePortForward
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }
    public string ForwardHost { get; set; }
    public int ForwardPort { get; set; }

    public string DroneDescription { get; set; }

    public static implicit operator ReversePortForward(ReversePortForwardResponse response)
    {
        if (response is null)
            return null;

        return new ReversePortForward
        {
            Id = response.Id,
            DroneId = response.DroneId,
            BindPort = response.BindPort,
            ForwardHost = response.ForwardHost,
            ForwardPort = response.ForwardPort
        };
    }
}