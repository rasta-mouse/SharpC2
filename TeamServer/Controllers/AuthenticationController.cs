using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[ApiController]
[AllowAnonymous]
[Route(Routes.V1.Auth)]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _auth;

    public AuthenticationController(IAuthenticationService auth)
    {
        _auth = auth;
    }

    [HttpPost]
    public ActionResult<AuthenticationResponse> Authenticate([FromBody] AuthenticationRequest request)
    {
        var result = _auth.AuthenticateUser(request.Nick, request.Pass, out var token);
        var response = new AuthenticationResponse
        {
            Success = result,
            Token = token
        };

        if (result) return Ok(response);
        return Unauthorized(response);
    }
}