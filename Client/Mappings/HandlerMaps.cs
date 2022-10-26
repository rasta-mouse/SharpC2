using AutoMapper;
using SharpC2.API.Response;
using SharpC2.Models;

namespace SharpC2.Mappings;

public sealed class HandlerMaps : Profile
{
    public HandlerMaps()
    {
        CreateMap<HandlerResponse, Handler>();
        
        CreateMap<HttpHandlerResponse, HttpHandler>()
            .IncludeAllDerived();
        
        CreateMap<SmbHandlerResponse, SmbHandler>()
            .IncludeAllDerived();
        
        CreateMap<TcpHandlerResponse, TcpHandler>()
            .IncludeAllDerived();
    }
}