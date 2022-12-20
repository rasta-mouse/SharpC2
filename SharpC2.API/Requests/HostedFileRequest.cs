namespace SharpC2.API.Requests;

public sealed class HostedFileRequest
{
    public string Handler { get; set; }
    public string Uri { get; set; }
    public string Filename { get; set; }
    public byte[] Bytes { get; set; }
}