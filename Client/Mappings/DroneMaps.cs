using AutoMapper;
using Client.Models.Drones;
using SharpC2.API.Responses;

namespace Client.Mappings;

public class DroneMaps : Profile
{
	public DroneMaps()
	{
		CreateMap<MetadataResponse, Metadata>();
		CreateMap<DroneResponse, Drone>();
	}
}