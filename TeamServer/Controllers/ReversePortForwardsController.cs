using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Drones;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Pivots;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.ReversePortForwards)]
public class ReversePortForwardsController : ControllerBase
{
    private readonly IReversePortForwardService _portForwards;
    private readonly IDroneService _drones;
    private readonly ITaskService _tasks;
    private readonly ICryptoService _crypto;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public ReversePortForwardsController(IReversePortForwardService portForwards, IDroneService drones,
        ITaskService tasks, ICryptoService crypto, IMapper mapper, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _portForwards = portForwards;
        _drones = drones;
        _tasks = tasks;
        _crypto = crypto;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReversePortForwardResponse>>> GetAll([FromQuery] string droneId)
    {
        IEnumerable<ReversePortForward> forwards;

        if (string.IsNullOrWhiteSpace(droneId))
            forwards = await _portForwards.GetAll();
        else
            forwards = await _portForwards.GetAll(droneId);

        var response = _mapper.Map<IEnumerable<ReversePortForward>, IEnumerable<ReversePortForwardResponse>>(forwards);
        return Ok(response);
    }

    [HttpGet("{forwardId}")]
    public async Task<ActionResult<ReversePortForwardResponse>> Get(string forwardId)
    {
        var forward = await _portForwards.Get(forwardId);

        if (forward is null)
            return NotFound();
        
        var response = _mapper.Map<ReversePortForward, ReversePortForwardResponse>(forward);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ReversePortForwardResponse>> CreateNew([FromBody] ReversePortForwardRequest request)
    {
        // check drone exists
        var drone = await _drones.Get(request.DroneId);

        if (drone is null)
            return NotFound("Drone not found");
        
        // check it's health
        if (drone.Status != DroneStatus.ALIVE)
            return BadRequest("Cannot create a port forward on an unhealthy Drone");
        
        // check to see if the bind port is already in use
        var existing = await _portForwards.GetAll(request.DroneId);

        if (existing.Any(f => f.BindPort == request.BindPort))
            return BadRequest("BindPort already in use");

        // create new forward
        var forward = new ReversePortForward
        {
            DroneId = request.DroneId,
            BindPort = request.BindPort,
            ForwardHost = request.ForwardHost,
            ForwardPort = request.ForwardPort
        };

        await _portForwards.Add(forward);
        await _hub.Clients.All.ReversePortForwardCreated(forward.Id);
        
        // task the drone
        var packet = new ReversePortForwardPacket(forward.Id, ReversePortForwardPacket.PacketType.START, BitConverter.GetBytes(forward.BindPort));
        _tasks.CacheFrame(forward.DroneId, new C2Frame(forward.DroneId, FrameType.REV_PORT_FWD, await _crypto.Encrypt(packet)));
        
        var response = _mapper.Map<ReversePortForward, ReversePortForwardResponse>(forward);
        return Ok(response);
    }

    [HttpDelete("{forwardId}")]
    public async Task<IActionResult> Delete(string forwardId)
    {
        var forward = await _portForwards.Get(forwardId);

        if (forward is null)
            return NotFound();

        await _portForwards.Delete(forward);
        await _hub.Clients.All.ReversePortForwardDeleted(forward.Id);
        
        // task the drone
        var packet = new ReversePortForwardPacket(forward.Id, ReversePortForwardPacket.PacketType.STOP);
        _tasks.CacheFrame(forward.DroneId, new C2Frame(forward.DroneId, FrameType.REV_PORT_FWD, await _crypto.Encrypt(packet)));
        
        return NoContent();
    }
}