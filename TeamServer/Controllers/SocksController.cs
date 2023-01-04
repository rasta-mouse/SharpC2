using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Pivots;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Socks)]
public sealed class SocksController : ControllerBase
{
    private readonly ISocksService _socks;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public SocksController(ISocksService socks, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _socks = socks;
        _hub = hub;
    }

    [HttpGet]
    public ActionResult<IEnumerable<SocksResponse>> GetSocksProxies()
    {
        var socks = _socks.Get();
        var response = socks.Select(s => (SocksResponse)s);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public ActionResult<SocksResponse> GetSocksProxy(string id)
    {
        var socks = _socks.Get(id);

        if (socks is null)
            return NotFound();

        return Ok((SocksResponse)socks);
    }

    [HttpPost]
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

    [HttpDelete("{id}")]
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