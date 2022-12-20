using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Handlers;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public sealed class HostedFileMaps : Profile
{
    public HostedFileMaps()
    {
        CreateMap<HostedFile, HostedFileResponse>();

        CreateMap<HostedFile, HostedFileDao>();
        CreateMap<HostedFileDao, HostedFile>();
    }
}