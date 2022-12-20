using AutoMapper;
using Client.Models.Events;
using SharpC2.API.Responses;

namespace Client.Mappings;

public sealed class EventMaps : Profile
{
    public EventMaps()
    {
        CreateMap<WebLogEventResponse, WebLogEvent>()
            .IncludeAllDerived();

        CreateMap<UserAuthEventResponse, UserAuthEvent>()
            .IncludeAllDerived();
    }
}