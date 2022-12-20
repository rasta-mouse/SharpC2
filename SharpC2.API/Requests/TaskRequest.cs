namespace SharpC2.API.Requests;

public class TaskRequest
{
    public byte Command { get; set; }
    public string Alias { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
    public int ResultType { get; set; }
}