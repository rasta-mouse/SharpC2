namespace SharpC2.Models;

public sealed class DroneTask
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public byte Command { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public DateTime StartTime { get; set; }
    public DroneTaskStatus Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }
}

public enum DroneTaskStatus
{
    Pending = 0,
    Tasked = 1,
    Running = 2,
    Complete = 3,
    Aborted = 4
}