using AutoMapper;
using Client.Models.Handlers;
using SharpC2.API.Responses;

namespace Client.Mappings;

public class HandlerMaps : Profile
{
    public HandlerMaps()
    {
        CreateMap<HttpHandlerResponse, HttpHandler>()
            .IncludeAllDerived();

        CreateMap<HostedFileResponse, HostedFile>();
    }
}