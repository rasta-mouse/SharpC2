using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Messages;
using TeamServer.Storage;
using TeamServer.Tasks;

namespace TeamServer.Mappings;

public sealed class TaskMaps : Profile
{
    public TaskMaps()
    {
        CreateMap<TaskRecord, TaskRecordResponse>();
        CreateMap<TaskRecord, DroneTask>();

        CreateMap<TaskRecord, TaskRecordDao>()
            .ForMember(t => t.Arguments, o => o.MapFrom(t => string.Join("__,__", t.Arguments)));

        CreateMap<TaskRecordDao, TaskRecord>()
            .ForMember(t => t.Arguments, o => o.MapFrom(t => t.Arguments.Split("__,__", StringSplitOptions.RemoveEmptyEntries)));
    }
}