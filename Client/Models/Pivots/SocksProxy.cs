using SharpC2.API.Responses;

namespace Client.Models.Pivots;

public class SocksProxy
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }

    public string DroneDescription { get; set; }

    public static implicit operator SocksProxy(SocksResponse response)
    {
        return new SocksProxy
        {
            Id = response.Id,
            DroneId = response.DroneId,
            BindPort = response.BindPort
        };
    }
}