using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Handlers;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class HandlerMapping : Profile
{
    public HandlerMapping()
    {
        CreateMap<HttpHandler, HttpHandlerDao>()
            .IncludeAllDerived();
        
        CreateMap<TcpHandler, TcpHandlerDao>()
            .IncludeAllDerived();

        CreateMap<SmbHandler, SmbHandlerDao>()
            .IncludeAllDerived();

        CreateMap<Handler, HandlerResponse>();
        CreateMap<HttpHandler, HttpHandlerResponse>();
        CreateMap<TcpHandler, TcpHandlerResponse>();
        CreateMap<SmbHandler, SmbHandlerResponse>();
    }
}