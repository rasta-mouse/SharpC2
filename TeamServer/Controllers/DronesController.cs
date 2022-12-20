using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Responses;

using TeamServer.Drones;
using TeamServer.Hubs;
using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Drones)]
public class DronesController : ControllerBase
{
    private readonly IDroneService _drones;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public DronesController(IDroneService drones, IMapper mapper, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _drones = drones;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<DroneResponse>> GetDrones()
    {
        var drones = await _drones.Get();
        var response = _mapper.Map<IEnumerable<Drone>, IEnumerable<DroneResponse>>(drones);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DroneResponse>> GetDrone(string id)
    {
        var drone = await _drones.Get(id);

        if (drone is null)
            return NotFound();

        var response = _mapper.Map<Drone, DroneResponse>(drone);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDrone(string id)
    {
        var drone = await _drones.Get(id);

        if (drone is null)
            return NotFound();

        await _drones.Delete(drone);
        await _hub.Clients.All.DroneDeleted(drone.Metadata.Id);
        
        return NoContent();
    }
}