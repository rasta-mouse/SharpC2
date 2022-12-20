using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Handlers;
using TeamServer.Hubs;
using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Handlers)]
public class HandlersController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IMapper _mapper;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public HandlersController(IHandlerService handlers, IMapper mapper, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _handlers = handlers;
        _mapper = mapper;
        _hub = hub;
    }
    
    [HttpGet("http")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandlers()
    {
        var handlers = _handlers.Get<HttpHandler>();
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HttpHandlerResponse>>(handlers);

        return Ok(response);
    }
    
    [HttpGet("http/{name}")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandler(string name)
    {
        var handler = _handlers.Get<HttpHandler>(name);

        if (handler is null)
            return NotFound();
        
        var response = _mapper.Map<HttpHandler, HttpHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpPost("http")]
    public async Task<ActionResult<HttpHandlerResponse>> CreateHttpHandler([FromBody] HttpHandlerRequest request)
    {
        var handler = new HttpHandler(request.Secure)
        {
            Name = request.Name,
            BindPort = request.BindPort,
            ConnectAddress = request.ConnectAddress,
            ConnectPort = request.ConnectPort
        };
        
        _ = handler.Start();
        
        await _handlers.Add(handler);
        await _hub.Clients.All.HttpHandlerCreated(handler.Name);

        var response = _mapper.Map<HttpHandler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteHandler(string name)
    {
        var handler = _handlers.Get<Handler>(name);
        
        if (handler is null)
            return NotFound();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                ((HttpHandler)handler).Stop();
                await _hub.Clients.All.HttpHandlerDeleted(handler.Name);
                break;
        }
        
        await _handlers.Delete(handler);
        return NoContent();
    }
}