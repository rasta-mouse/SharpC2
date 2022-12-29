using AutoMapper;
using Client.Models.Pivots;
using SharpC2.API.Responses;

namespace Client.Mappings;

public sealed class PivotMaps : Profile
{
    public PivotMaps()
    {
        CreateMap<ReversePortForwardResponse, ReversePortForward>();
    }
}