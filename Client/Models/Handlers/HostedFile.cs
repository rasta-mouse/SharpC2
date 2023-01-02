using SharpC2.API.Responses;

namespace Client.Models.Handlers;

public sealed class HostedFile
{
    public string Id { get; set; }
    public string Handler { get; set; }
    public string Uri { get; set; }
    public string Filename { get; set; }
    public long Size { get; set; }

    public static implicit operator HostedFile(HostedFileResponse response)
    {
        if (response is null)
            return null;

        return new HostedFile
        {
            Id = response.Id,
            Handler = response.Handler,
            Uri = response.Uri,
            Filename = response.Filename,
            Size = response.Size
        };
    }
}