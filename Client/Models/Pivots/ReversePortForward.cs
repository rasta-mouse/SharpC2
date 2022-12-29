namespace Client.Models.Pivots;

public sealed class ReversePortForward
{
    public string Id { get; set; }
    public string DroneId { get; set; }
    public int BindPort { get; set; }
    public string ForwardHost { get; set; }
    public int ForwardPort { get; set; }

    public string DroneDescription { get; set; }
}