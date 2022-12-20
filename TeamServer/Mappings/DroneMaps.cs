using AutoMapper;

using SharpC2.API.Responses;

using TeamServer.Drones;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class DroneMaps : Profile
{
    public DroneMaps()
    {
        CreateMap<Metadata, MetadataResponse>();
        CreateMap<Drone, DroneResponse>();

        CreateMap<Drone, DroneDao>()
            .ForMember(d => d.Id, o => o.MapFrom(d => d.Metadata.Id))
            .ForMember(d => d.Identity, o => o.MapFrom(d => d.Metadata.Identity))
            .ForMember(d => d.Address, o => o.MapFrom(d => d.Metadata.Address))
            .ForMember(d => d.Hostname, o => o.MapFrom(d => d.Metadata.Hostname))
            .ForMember(d => d.Process, o => o.MapFrom(d => d.Metadata.Process))
            .ForMember(d => d.Pid, o => o.MapFrom(d => d.Metadata.Pid))
            .ForMember(d => d.Is64Bit, o => o.MapFrom(d => d.Metadata.Is64Bit))
            .ForMember(d => d.Integrity, o => o.MapFrom(d => (int)d.Metadata.Integrity));

        CreateMap<DroneDao, Drone>()
            .ForPath(d => d.Metadata.Id, o => o.MapFrom(d => d.Id))
            .ForPath(d => d.Metadata.Identity, o => o.MapFrom(d => d.Identity))
            .ForPath(d => d.Metadata.Address, o => o.MapFrom(d => d.Address))
            .ForPath(d => d.Metadata.Hostname, o => o.MapFrom(d => d.Hostname))
            .ForPath(d => d.Metadata.Process, o => o.MapFrom(d => d.Process))
            .ForPath(d => d.Metadata.Pid, o => o.MapFrom(d => d.Pid))
            .ForPath(d => d.Metadata.Is64Bit, o => o.MapFrom(d => d.Is64Bit))
            .ForPath(d => d.Metadata.Integrity, o => o.MapFrom(d => (IntegrityLevel)d.Integrity));
    }
}