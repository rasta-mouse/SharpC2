using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Events;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class EventMaps : Profile
{
    public EventMaps()
    {
        CreateMap<WebLogEvent, WebLogEventResponse>().IncludeAllDerived();
        CreateMap<UserAuthEvent, UserAuthEventResponse>().IncludeAllDerived();

        CreateMap<WebLogEvent, WebLogDao>().IncludeAllDerived();
        CreateMap<WebLogDao, WebLogEvent>().IncludeAllDerived();

        CreateMap<UserAuthEvent, UserAuthDao>().IncludeAllDerived();
        CreateMap<UserAuthDao, UserAuthEvent>().IncludeAllDerived();
    }
}