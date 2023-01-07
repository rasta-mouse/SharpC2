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
[Route(Routes.V1.Pivots)]
public sealed class PivotsController : ControllerBase
{
    private readonly IReversePortForwardService _portForwards;
    private readonly ISocksService _socks;
    private readonly IDroneService _drones;
    private readonly ITaskService _tasks;
    private readonly ICryptoService _crypto;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;
    
    public PivotsController(IReversePortForwardService portForwards, IDroneService drones,
        ITaskService tasks, ICryptoService crypto, IHubContext<NotificationHub, INotificationHub> hub, ISocksService socks)
    {
        _portForwards = portForwards;
        _drones = drones;
        _tasks = tasks;
        _crypto = crypto;
        _hub = hub;
        _socks = socks;
    }
    
    [HttpGet("rportfwd")]
    public async Task<ActionResult<IEnumerable<ReversePortForwardResponse>>> GetAll([FromQuery] string droneId)
    {
        IEnumerable<ReversePortForward> forwards;

        if (string.IsNullOrWhiteSpace(droneId))
            forwards = await _portForwards.GetAll();
        else
            forwards = await _portForwards.GetAll(droneId);

        var response = forwards.Select(f => (ReversePortForwardResponse)f);
        return Ok(response);
    }

    [HttpGet("rportfwd/{id}")]
    public async Task<ActionResult<ReversePortForwardResponse>> Get(string id)
    {
        var forward = await _portForwards.Get(id);

        if (forward is null)
            return NotFound();
        
        return Ok((ReversePortForwardResponse)forward);
    }

    [HttpPost("rportfwd")]
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
        var forward = (ReversePortForward)request;

        await _portForwards.Add(forward);
        await _hub.Clients.All.ReversePortForwardCreated(forward.Id);
        
        // task the drone
        var packet = new ReversePortForwardPacket(forward.Id, ReversePortForwardPacket.PacketType.START, BitConverter.GetBytes(forward.BindPort));
        var frame = new C2Frame(forward.DroneId, FrameType.REV_PORT_FWD, await _crypto.Encrypt(packet));
        _tasks.CacheFrame(frame);
        
        return Ok((ReversePortForwardResponse)forward);
    }

    [HttpDelete("rportfwd/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var forward = await _portForwards.Get(id);

        if (forward is null)
            return NotFound();

        await _portForwards.Delete(forward);
        await _hub.Clients.All.ReversePortForwardDeleted(forward.Id);
        
        // task the drone
        var packet = new ReversePortForwardPacket(forward.Id, ReversePortForwardPacket.PacketType.STOP);
        var frame = new C2Frame(forward.DroneId, FrameType.REV_PORT_FWD, await _crypto.Encrypt(packet));
        _tasks.CacheFrame(frame);
        
        return NoContent();
    }
    
    [HttpGet("socks")]
    public ActionResult<IEnumerable<SocksResponse>> GetSocksProxies()
    {
        var socks = _socks.Get();
        var response = socks.Select(s => (SocksResponse)s);

        return Ok(response);
    }

    [HttpGet("socks/{id}")]
    public ActionResult<SocksResponse> GetSocksProxy(string id)
    {
        var socks = _socks.Get(id);

        if (socks is null)
            return NotFound();

        return Ok((SocksResponse)socks);
    }

    [HttpPost("socks")]
    public async Task<ActionResult<SocksResponse>> CreateSocksProxy([FromBody] SocksRequest request)
    {
        var existing = _socks.Get();

        if (existing.Any(s => s.BindPort == request.BindPort))
            return BadRequest("Bind Port is already in use");

        var socks = (SocksProxy)request;
        _ = socks.Start();
        
        await _socks.Add(socks);
        await _hub.Clients.All.SocksProxyStarted(socks.Id);
        
        return Ok((SocksResponse)socks);
    }

    [HttpDelete("socks/{id}")]
    public async Task<IActionResult> DeleteSocksProxy(string id)
    {
        var socks = _socks.Get(id);

        if (socks is null)
            return NotFound();
        
        socks.Stop();

        await _socks.Delete(socks);
        await _hub.Clients.All.SocksProxyStopped(socks.Id);
        
        return NoContent();
    }
}