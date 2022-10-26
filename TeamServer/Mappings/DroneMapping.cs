using AutoMapper;

using SharpC2.API.Response;

using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Mappings;

public class DroneMapping : Profile
{
    public DroneMapping()
    {
        CreateMap<Drone, DroneDao>()
            .ForMember(d => d.Id, o => o.MapFrom(d => d.Metadata.Id))
            .ForMember(d => d.InternalAddress, o => o.MapFrom(d => d.Metadata.Address))
            .ForMember(d => d.Identity, o => o.MapFrom(d => d.Metadata.Identity))
            .ForMember(d => d.Hostname, o => o.MapFrom(d => d.Metadata.Hostname))
            .ForMember(d => d.Process, o => o.MapFrom(d => d.Metadata.Process))
            .ForMember(d => d.ProcessId, o => o.MapFrom(d => d.Metadata.ProcessId))
            .ForMember(d => d.Architecture, o => o.MapFrom(d => d.Metadata.Architecture))
            .ForMember(d => d.Integrity, o => o.MapFrom(d => d.Metadata.Integrity));

        CreateMap<DroneDao, Drone>()
            .ForPath(d => d.Metadata.Id, o => o.MapFrom(d => d.Id))
            .ForPath(d => d.Metadata.Address, o => o.MapFrom(d => d.InternalAddress))
            .ForPath(d => d.Metadata.Identity, o => o.MapFrom(d => d.Identity))
            .ForPath(d => d.Metadata.Hostname, o => o.MapFrom(d => d.Hostname))
            .ForPath(d => d.Metadata.Process, o => o.MapFrom(d => d.Process))
            .ForPath(d => d.Metadata.ProcessId, o => o.MapFrom(d => d.ProcessId))
            .ForPath(d => d.Metadata.Architecture, o => o.MapFrom(d => d.Architecture))
            .ForPath(d => d.Metadata.Integrity, o => o.MapFrom(d => d.Integrity));

        CreateMap<Drone, DroneResponse>();
        CreateMap<DroneMetadata, MetadataResponse>();
    }
}