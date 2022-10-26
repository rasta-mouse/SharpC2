using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class TaskMapping : Profile
{
    public TaskMapping()
    {
        CreateMap<DroneTaskRecord, DroneTaskRecordDao>()
            .ForMember(t =>
                t.Arguments, o =>
                o.MapFrom(t => string.Join("__,__", t.Arguments)));

        CreateMap<DroneTaskRecordDao, DroneTaskRecord>()
            .ForMember(t =>
                t.Arguments, o =>
                o.MapFrom(t => t.Arguments.Split("__,__", StringSplitOptions.RemoveEmptyEntries)));

        CreateMap<DroneTaskRecord, DroneTaskResponse>();
        CreateMap<DroneTaskRecord, DroneTask>();
    }
}