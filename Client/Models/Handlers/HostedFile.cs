namespace Client.Models.Handlers;

public sealed class HostedFile
{
    public string Id { get; set; }
    public string Handler { get; set; }
    public string Uri { get; set; }
    public string Filename { get; set; }
    public long Size { get; set; }
}