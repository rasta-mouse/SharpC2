namespace SharpC2.API.Response;

public sealed class DroneTaskResponse
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public byte Command { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public DateTime StartTime { get; set; }
    public int Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }
}