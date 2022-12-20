using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;
using SharpC2.API.Responses;

using TeamServer.Events;
using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Events)]
public class EventsController : ControllerBase
{
    private readonly IEventService _events;
    private readonly IMapper _mapper;

    public EventsController(IEventService events, IMapper mapper)
    {
        _events = events;
        _mapper = mapper;
    }
    
    [HttpGet("auth")]
    public async Task<ActionResult<IEnumerable<UserAuthEventResponse>>> GetAuthLogs()
    {
        var events = await _events.Get<UserAuthEvent>();
        var response = _mapper.Map<IEnumerable<UserAuthEvent>, IEnumerable<UserAuthEventResponse>>(events);

        return Ok(response);
    }
    
    [HttpGet("auth/{id}")]
    public async Task<ActionResult<UserAuthEventResponse>> GetAuthLog(string id)
    {
        var ev = await _events.Get<UserAuthEvent>(id);

        if (ev is null)
            return NotFound();
        
        var response = _mapper.Map<UserAuthEvent, UserAuthEventResponse>(ev);
        return Ok(response);
    }

    [HttpGet("web")]
    public async Task<ActionResult<IEnumerable<WebLogEventResponse>>> GetWebLogs()
    {
        var events = await _events.Get<WebLogEvent>();
        var response = _mapper.Map<IEnumerable<WebLogEvent>, IEnumerable<WebLogEventResponse>>(events);

        return Ok(response);
    }
    
    [HttpGet("web/{id}")]
    public async Task<ActionResult<WebLogEventResponse>> GetWebLog(string id)
    {
        var ev = await _events.Get<WebLogEvent>(id);

        if (ev is null)
            return NotFound();
        
        var response = _mapper.Map<WebLogEvent, WebLogEventResponse>(ev);
        return Ok(response);
    }
}