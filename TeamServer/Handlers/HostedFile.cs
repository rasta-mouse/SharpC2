using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Utilities;

namespace TeamServer.Handlers;

public sealed class HostedFile
{
    public string Id { get; set; }
    public string Handler { get; set; }
    public string Uri { get; set; }
    public string Filename { get; set; }
    public long Size { get; set; }

    public static implicit operator HostedFile(HostedFileRequest request)
    {
        return new HostedFile
        {
            Id = Helpers.GenerateShortGuid(),
            Handler = request.Handler,
            Uri = request.Uri,
            Filename = request.Filename,
            Size = request.Bytes.LongLength
        };
    }

    public static implicit operator HostedFileResponse(HostedFile file)
    {
        return new HostedFileResponse
        {
            Id = file.Id,
            Handler = file.Handler,
            Uri = file.Uri,
            Filename = file.Filename,
            Size = file.Size
        };
    }
}