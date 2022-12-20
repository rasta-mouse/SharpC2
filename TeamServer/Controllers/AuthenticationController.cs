using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Events;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Utilities;

namespace TeamServer.Controllers;

[ApiController]
[Route(Routes.V1.Authentication)]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _auth;
    private readonly IEventService _events;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public AuthenticationController(IAuthenticationService auth, IEventService events, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _auth = auth;
        _events = events;
        _hub = hub;
    }

    [HttpPost]
    public async Task< ActionResult<AuthenticationResponse>> Authenticate([FromBody] AuthenticationRequest request)
    {
        var result = _auth.AuthenticateUser(request.Nick, request.Pass, out var token);
        
        var response = new AuthenticationResponse
        {
            Success = result,
            Token = token
        };

        var ev = new UserAuthEvent
        {
            Id = Helpers.GenerateShortGuid(),
            Date = DateTime.UtcNow,
            Nick = request.Nick,
            Result = result
        };

        await _events.Add(ev);
        await _hub.Clients.All.NewEvent(ev.Type, ev.Id);

        if (result) return Ok(response);
        return Unauthorized(response);
    } 
}