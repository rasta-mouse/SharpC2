namespace SharpC2.API.Responses;

public sealed class SocksResponse
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }
}