namespace SharpC2.API.Responses;

public class TaskRecordResponse
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public string Nick { get; set; }
    public byte Command { get; set; }
    public string Alias { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Status { get; set; }
    public byte[] Result { get; set; }
    public int ResultType { get; set; }
}