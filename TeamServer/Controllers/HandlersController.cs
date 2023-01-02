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
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public HandlersController(IHandlerService handlers, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _handlers = handlers;
        _hub = hub;
    }
    
    [HttpGet("http")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandlers()
    {
        var handlers = _handlers.Get<HttpHandler>();
        var response = handlers.Select(h => (HttpHandlerResponse)h);

        return Ok(response);
    }
    
    [HttpGet("tcp")]
    public ActionResult<IEnumerable<TcpHandlerResponse>> GetTcpHandlers()
    {
        var handlers = _handlers.Get<TcpHandler>();
        var response = handlers.Select(h => (TcpHandlerResponse)h);

        return Ok(response);
    }
    
    [HttpGet("smb")]
    public ActionResult<IEnumerable<SmbHandlerResponse>> GetSmbHandlers()
    {
        var handlers = _handlers.Get<SmbHandler>();
        var response = handlers.Select(h => (SmbHandlerResponse)h);

        return Ok(response);
    }
    
    [HttpGet("ext")]
    public ActionResult<IEnumerable<ExtHandlerResponse>> GetExternalHandlers()
    {
        var handlers = _handlers.Get<ExtHandler>();
        var response = handlers.Select(h => (ExtHandlerResponse)h);

        return Ok(response);
    }
    
    [HttpGet("http/{name}")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandler(string name)
    {
        var handler = _handlers.Get<HttpHandler>(name);

        if (handler is null)
            return NotFound();
        
        return Ok((HttpHandlerResponse)handler);
    }
    
    [HttpGet("tcp/{name}")]
    public ActionResult<IEnumerable<TcpHandlerResponse>> GetTcpHandler(string name)
    {
        var handler = _handlers.Get<TcpHandler>(name);

        if (handler is null)
            return NotFound();
        
        return Ok((TcpHandlerResponse)handler);
    }
    
    [HttpGet("smb/{name}")]
    public ActionResult<IEnumerable<SmbHandlerResponse>> GetSmbHandler(string name)
    {
        var handler = _handlers.Get<SmbHandler>(name);

        if (handler is null)
            return NotFound();
        
        return Ok((SmbHandlerResponse)handler);
    }
    
    [HttpGet("ext/{name}")]
    public ActionResult<IEnumerable<ExtHandlerResponse>> GetExternalHandler(string name)
    {
        var handler = _handlers.Get<ExtHandler>(name);

        if (handler is null)
            return NotFound();
        
        return Ok((ExtHandlerResponse)handler);
    }
    
    [HttpPost("http")]
    public async Task<ActionResult<HttpHandlerResponse>> CreateHttpHandler([FromBody] HttpHandlerRequest request)
    {
        var handler = (HttpHandler)request;
        _ = handler.Start();
        
        await _handlers.Add(handler);
        await _hub.Clients.All.HttpHandlerCreated(handler.Name);

        return Ok((HttpHandlerResponse)handler);
    }
    
    [HttpPost("tcp")]
    public async Task<ActionResult<TcpHandlerResponse>> CreateTcpHandler([FromBody] TcpHandlerRequest request)
    {
        var handler = (TcpHandler)request;

        await _handlers.Add(handler);
        await _hub.Clients.All.TcpHandlerCreated(handler.Name);

        return Ok((TcpHandlerResponse)handler);
    }
    
    [HttpPost("smb")]
    public async Task<ActionResult<SmbHandlerResponse>> CreateSmbHandler([FromBody] SmbHandlerRequest request)
    {
        var handler = (SmbHandler)request;

        await _handlers.Add(handler);
        await _hub.Clients.All.SmbHandlerCreated(handler.Name);

        return Ok((SmbHandlerResponse)handler);
    }
    
    [HttpPost("ext")]
    public async Task<ActionResult<ExtHandlerResponse>> CreateExternalHandler([FromBody] ExtHandlerRequest request)
    {
        var handler = (ExtHandler)request;
        _ = handler.Start();
        
        await _handlers.Add(handler);
        await _hub.Clients.All.ExternalHandlerCreated(handler.Name);

        return Ok((ExtHandlerResponse)handler);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteHandler(string name)
    {
        var handler = _handlers.Get<Handler>(name);
        
        if (handler is null)
            return NotFound();
        
        await _handlers.Delete(handler);

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                ((HttpHandler)handler).Stop();
                await _hub.Clients.All.HttpHandlerDeleted(handler.Name);
                break;

            case HandlerType.SMB:
                await _hub.Clients.All.SmbHandlerDeleted(handler.Name);
                break;
                
            case HandlerType.TCP:
                await _hub.Clients.All.TcpHandlerDeleted(handler.Name);
                break;
            
            case HandlerType.EXTERNAL:
                await _hub.Clients.All.ExternalHandlerDeleted(handler.Name);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        return NoContent();
    }
}