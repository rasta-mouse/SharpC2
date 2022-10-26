namespace SharpC2.API.Request;

public sealed class DroneTaskRequest
{
    public string DroneId { get; set; }
    public string Alias { get; set; }
    public byte Command { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
}