using AutoMapper;
using Client.Models.Handlers;
using SharpC2.API.Responses;

namespace Client.Mappings;

public class HandlerMaps : Profile
{
    public HandlerMaps()
    {
        CreateMap<HttpHandlerResponse, HttpHandler>().IncludeAllDerived();
        CreateMap<SmbHandlerResponse, SmbHandler>().IncludeAllDerived();
        CreateMap<TcpHandlerResponse, TcpHandler>().IncludeAllDerived();
        CreateMap<ExternalHandlerResponse, ExtHandler>().IncludeAllDerived();
        CreateMap<HostedFileResponse, HostedFile>();
    }
}