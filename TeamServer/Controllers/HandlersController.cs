using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Handlers)]
public sealed class HandlersController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public HandlersController(IHandlerService handlers, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _handlers = handlers;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public ActionResult<IEnumerable<HandlerResponse>> GetAllHandlers()
    {
        var handlers = _handlers.GetHandlers<Handler>();
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HandlerResponse>>(handlers);

        return Ok(response);
    }

    [HttpGet("http")]
    public ActionResult<IEnumerable<HttpHandlerResponse>> GetHttpHandlers()
    {
        var handlers = _handlers.GetHandlers<HttpHandler>(HandlerType.HTTP);
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HttpHandlerResponse>>(handlers);

        return Ok(response);
    }

    [HttpGet("tcp")]
    public ActionResult<IEnumerator<TcpHandlerResponse>> GetTcpHandlers()
    {
        var handlers = _handlers.GetHandlers<TcpHandler>(HandlerType.TCP);
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<TcpHandlerResponse>>(handlers);

        return Ok(response);
    }
    
    [HttpGet("smb")]
    public ActionResult<IEnumerator<TcpHandlerResponse>> GetSmbHandlers()
    {
        var handlers = _handlers.GetHandlers<SmbHandler>(HandlerType.SMB);
        var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<SmbHandlerResponse>>(handlers);

        return Ok(response);
    }

    [HttpGet("http/{name}")]
    public ActionResult<HttpHandlerResponse> GetHttpHandler(string name)
    {
        var handler = _handlers.GetHandler<HttpHandler>(name);
        
        if (handler is null)
            return NotFound();

        var response = _mapper.Map<Handler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpGet("tcp/{name}")]
    public ActionResult<TcpHandlerResponse> GetTcpHandler(string name)
    {
        var handler = _handlers.GetHandler<TcpHandler>(name);
        
        if (handler is null)
            return NotFound();

        var response = _mapper.Map<Handler, TcpHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpGet("smb/{name}")]
    public ActionResult<TcpHandlerResponse> GetSmbHandler(string name)
    {
        var handler = _handlers.GetHandler<SmbHandler>(name);
        
        if (handler is null)
            return NotFound();

        var response = _mapper.Map<Handler, SmbHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpPost("http")]
    public async Task<ActionResult<HttpHandlerResponse>> CreateHttpHandler([FromBody] CreateHttpHandlerRequest request)
    {
        // check existing handlers don't exist for name and port
        var currentHandlers = _handlers.GetHandlers<HttpHandler>(HandlerType.HTTP).ToArray();

        if (currentHandlers.Any(h => h.Name.Equals(request.Name)))
            return BadRequest($"Handler with name \"{request.Name}\" already exists.");

        if (currentHandlers.Any(h => h.BindPort == request.BindPort))
            return BadRequest($"Handler with bind port \"{request.BindPort}\" already exists.");

        // create and start
        var handler = new HttpHandler(request.Name, request.BindPort, request.ConnectAddress, request.ConnectPort, request.Secure);
        _ = handler.Start();
        
        // store
        await _handlers.AddHandler(handler);
        
        // notify hub
        await _hub.Clients.All.NotifyHttpHandlerCreated(handler.Name);

        // return to user
        var response = _mapper.Map<HttpHandler, HttpHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpPost("tcp")]
    public async Task<ActionResult<TcpHandlerResponse>> CreateTcpHandler([FromBody] CreateTcpHandlerRequest request)
    {
        // create
        var handler = new TcpHandler(request.Name, request.BindPort, request.LoopbackOnly);

        // store
        await _handlers.AddHandler(handler);
        
        // notify hub
        await _hub.Clients.All.NotifyTcpHandlerCreated(handler.Name);

        // return to user
        var response = _mapper.Map<TcpHandler, TcpHandlerResponse>(handler);
        return Ok(response);
    }
    
    [HttpPost("smb")]
    public async Task<ActionResult<SmbHandlerResponse>> CreateSmbHandler([FromBody] CreateSmbHandlerRequest request)
    {
        // create
        var handler = new SmbHandler(request.Name, request.PipeName);

        // store
        await _handlers.AddHandler(handler);
        
        // notify hub
        await _hub.Clients.All.NotifySmbHandlerCreated(handler.Name);

        // return to user
        var response = _mapper.Map<SmbHandler, SmbHandlerResponse>(handler);
        return Ok(response);
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteHandler(string name)
    {
        var handler = _handlers.GetHandler<Handler>(name);
        
        if (handler is null)
            return NotFound();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                ((HttpHandler)handler).Stop();
                await _hub.Clients.All.NotifyHttpHandlerDeleted(handler.Name);
                break;
            
            case HandlerType.TCP:
                await _hub.Clients.All.NotifyTcpHandlerDeleted(handler.Name);
                break;
            
            case HandlerType.SMB:
                await _hub.Clients.All.NotifySmbHandlerDeleted(handler.Name);
                break;
            
            case HandlerType.EXTERNAL:
                break;
        }
        
        await _handlers.DeleteHandler(handler);
        return NoContent();
    }
}