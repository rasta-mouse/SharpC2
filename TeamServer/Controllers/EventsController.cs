using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;
using SharpC2.API.Responses;

using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Events)]
public class EventsController : ControllerBase
{
    private readonly IEventService _events;

    public EventsController(IEventService events)
    {
        _events = events;
    }
    
    [HttpGet("auth")]
    public async Task<ActionResult<IEnumerable<UserAuthEventResponse>>> GetAuthLogs()
    {
        var events = await _events.GetAuthEvents();
        var response = events.Select(e => (UserAuthEventResponse)e);

        return Ok(response);
    }
    
    [HttpGet("auth/{id}")]
    public async Task<ActionResult<UserAuthEventResponse>> GetAuthLog(string id)
    {
        var ev = await _events.GetAuthEvent(id);

        if (ev is null)
            return NotFound();
        
        return Ok((UserAuthEventResponse)ev);
    }

    [HttpGet("web")]
    public async Task<ActionResult<IEnumerable<WebLogEventResponse>>> GetWebLogs()
    {
        var events = await _events.GetWebLogEvents();
        var response = events.Select(e => (WebLogEventResponse)e);

        return Ok(response);
    }
    
    [HttpGet("web/{id}")]
    public async Task<ActionResult<WebLogEventResponse>> GetWebLog(string id)
    {
        var ev = await _events.GetWebLogEvent(id);

        if (ev is null)
            return NotFound();
        
        return Ok((WebLogEventResponse)ev);
    }
}