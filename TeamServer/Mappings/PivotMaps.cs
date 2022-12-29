using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Pivots;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public sealed class PivotMaps : Profile
{
    public PivotMaps()
    {
        CreateMap<ReversePortForward, ReversePortForwardDao>();
        CreateMap<ReversePortForwardDao, ReversePortForward>();
        CreateMap<ReversePortForward, ReversePortForwardResponse>();
    }
}