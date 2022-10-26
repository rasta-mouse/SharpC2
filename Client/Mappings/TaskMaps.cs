using AutoMapper;
using SharpC2.API.Response;
using SharpC2.Models;

namespace SharpC2.Mappings;

public sealed class TaskMaps : Profile
{
    public TaskMaps()
    {
        CreateMap<DroneTaskResponse, DroneTask>();
    }
}