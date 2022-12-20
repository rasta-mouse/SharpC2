using AutoMapper;
using Client.Models.Tasks;
using SharpC2.API.Responses;

namespace Client.Mappings;

public class TaskMaps : Profile
{
    public TaskMaps()
    {
        CreateMap<TaskRecordResponse, TaskRecord>();
    }
}