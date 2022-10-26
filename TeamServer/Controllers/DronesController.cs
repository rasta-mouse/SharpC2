using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Response;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Drones)]
public sealed class DronesController : ControllerBase
{
    private readonly IDroneService _drones;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public DronesController(IDroneService drones, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _drones = drones;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DroneResponse>>> GetDrones()
    {
        var drones = await _drones.GetDrones();
        var response = _mapper.Map<IEnumerable<Drone>, IEnumerable<DroneResponse>>(drones);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DroneResponse>> GetDrone(string id)
    {
        var drone = await _drones.GetDrone(id);
        
        if (drone is null)
            return NotFound();

        var response = _mapper.Map<Drone, DroneResponse>(drone);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDrone(string id)
    {
        var drone = await _drones.GetDrone(id);
        
        if (drone is null)
            return NotFound();

        await _drones.DeleteDrone(drone);
        await _hub.Clients.All.NotifyDroneRemoved(drone.Metadata.Id);
        
        // get all child drones
        var children = _drones.DepthFirstSearch(drone.Metadata.Id).ToArray();

        // 1st entry is current
        // 2nd entry is immediate child
        if (children.Length > 1)
        {
            var childId = children[1];
            var child = await _drones.GetDrone(childId);

            if (child is not null)
            {
                child.Parent = null;
                await _drones.UpdateDrone(child);
                
                _drones.DeleteEdge(drone.Metadata.Id, child.Metadata.Id);
            }
        }

        _drones.DeleteVertex(id);
        return NoContent();
    }
}