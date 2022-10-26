using AutoMapper;
using SharpC2.API.Response;
using SharpC2.Models;

namespace SharpC2.Mappings;

public sealed class DroneMaps : Profile
{
    public DroneMaps()
    {
        CreateMap<MetadataResponse, Metadata>();
        CreateMap<DroneResponse, Drone>();
    }
}