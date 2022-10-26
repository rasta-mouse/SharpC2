namespace SharpC2.API.Response;

public class MetadataResponse
{
    public string Id { get; set; }
    public string Address { get; set; }
    public string Identity { get; set; }
    public string Hostname { get; set; }
    public string Process { get; set; }
    public int ProcessId { get; set; }
    public int Integrity { get; set; }
    public int Architecture { get; set; }
}