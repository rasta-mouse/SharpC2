namespace SharpC2.API.Responses;

public class WebLogEventResponse
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Method { get; set; }
    public string Uri { get; set; }
    public string UserAgent { get; set; }
    public string SourceAddress { get; set; }
    public int ResponseCode { get; set; }
}