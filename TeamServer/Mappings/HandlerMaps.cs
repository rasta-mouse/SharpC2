using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Handlers;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class HandlerMaps : Profile
{
    public HandlerMaps()
    {
        CreateMap<HttpHandler, HttpHandlerResponse>();
        CreateMap<HttpHandler, HttpHandlerDao>();

        CreateMap<TcpHandler, TcpHandlerResponse>();
        CreateMap<TcpHandler, TcpHandlerDao>();

        CreateMap<SmbHandler, SmbHandlerResponse>();
        CreateMap<SmbHandler, SmbHandlerDao>();
    }
}