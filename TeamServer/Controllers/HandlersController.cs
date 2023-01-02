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
using TeamServer.Utilities;

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
        var response = _mapper.Map<IEnumerable<HttpHandler>, IEnumerable<HttpHandlerResponse>>(handlers);

        return Ok(response);
    }
    
    [HttpGet("tcp")]
    public ActionResult<IEnumerable<TcpHandlerResponse>> GetTcpHandlers()
    {
        var handlers = _handlers.Get<TcpHandler>();
        var response = _mapper.Map<IEnumerable<TcpHandler>, IEnumerable<TcpHandlerResponse>>(handlers);

        return Ok(response);
    }
    
    [HttpGet("smb")]
    public ActionResult<IEnumerable<SmbHandlerResponse>> GetSmbHandlers()
    {
        var handlers = _handlers.Get<SmbHandler>();
        var response = _mapper.Map<IEnumerable<SmbHandler>, IEnumerable<SmbHandlerResponse>>(handlers);

        return Ok(response);
    }
    
    [HttpGet("ext")]
    public ActionResult<IEnumerable<ExternalHandlerResponse>> GetExternalHandlers()
    {
        var handlers = _handlers.Get<ExternalHandler>();
        var response = _mapper.Map<IEnumerable<ExternalHandler>, IEnumerable<ExternalHandlerResponse>>(handlers);

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
    
    [HttpGet("tcp/{name}")]
    public ActionResult<IEnumerable<TcpHandlerResponse>> GetTcpHandler(string name)
    {
        var handler = _handlers.Get<TcpHandler>(name);

        if (handler is null)
            return NotFound();
        
        var response = _mapper.Map<TcpHandler, TcpHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpGet("smb/{name}")]
    public ActionResult<IEnumerable<SmbHandlerResponse>> GetSmbHandler(string name)
    {
        var handler = _handlers.Get<SmbHandler>(name);

        if (handler is null)
            return NotFound();
        
        var response = _mapper.Map<SmbHandler, SmbHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpGet("ext/{name}")]
    public ActionResult<IEnumerable<ExternalHandlerResponse>> GetExternalHandler(string name)
    {
        var handler = _handlers.Get<ExternalHandler>(name);

        if (handler is null)
            return NotFound();
        
        var response = _mapper.Map<ExternalHandler, ExternalHandlerResponse>(handler);
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
    
    [HttpPost("tcp")]
    public async Task<ActionResult<TcpHandlerResponse>> CreateTcpHandler([FromBody] TcpHandlerRequest request)
    {
        var handler = new TcpHandler
        {
            Name = request.Name,
            Address = request.Address,
            Port = request.Port,
            Loopback = request.Loopback
        };

        await _handlers.Add(handler);
        await _hub.Clients.All.TcpHandlerCreated(handler.Name);

        var response = _mapper.Map<TcpHandler, TcpHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpPost("smb")]
    public async Task<ActionResult<TcpHandlerResponse>> CreateSmbHandler([FromBody] SmbHandlerRequest request)
    {
        var handler = new SmbHandler
        {
            Name = request.Name,
            PipeName = request.PipeName
        };

        await _handlers.Add(handler);
        await _hub.Clients.All.SmbHandlerCreated(handler.Name);

        var response = _mapper.Map<SmbHandler, SmbHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpPost("ext")]
    public async Task<ActionResult<ExternalHandlerResponse>> CreateExternalHandler([FromBody] ExternalHandlerRequest request)
    {
        var handler = new ExternalHandler
        {
            Id = Helpers.GenerateShortGuid(),
            Name = request.Name,
            BindPort = request.BindPort
        };

        _ = handler.Start();
        
        await _handlers.Add(handler);
        await _hub.Clients.All.ExternalHandlerCreated(handler.Name);

        var response = _mapper.Map<ExternalHandler, ExternalHandlerResponse>(handler);
        return Ok(response);
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