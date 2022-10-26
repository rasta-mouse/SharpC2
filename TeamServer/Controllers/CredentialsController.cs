using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Credentials)]
public sealed class CredentialsController : ControllerBase
{
    private readonly ICredentialService _credentials;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public CredentialsController(ICredentialService credentials, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _credentials = credentials;
        _mapper = mapper;
        _hub = hub;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CredentialResponse>>> GetCredentials()
    {
        var credentials = await _credentials.GetCredentials();
        var response = _mapper.Map<IEnumerable<Credential>, IEnumerable<CredentialResponse>>(credentials);

        return Ok(response);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CredentialResponse>> GetCredential(string id)
    {
        var credential = await _credentials.GetCredential(id);
        
        if (credential is null)
            return NotFound();

        var response = _mapper.Map<Credential, CredentialResponse>(credential);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CredentialResponse>> CreateCredential([FromBody] CreateCredentialRequest request)
    {
        var credential = new Credential
        {
            Domain = request.Domain,
            Username = request.Username,
            Password = request.Password
        };

        await _credentials.AddCredential(credential);
        await _hub.Clients.All.NotifyCredentialAdded(credential.Id);

        var response = _mapper.Map<Credential, CredentialResponse>(credential);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CredentialResponse>> UpdateCredential(string id, [FromBody] CreateCredentialRequest request)
    {
        var credential = await _credentials.GetCredential(id);

        if (credential is null)
            return NotFound();

        credential.Domain = request.Domain;
        credential.Username = request.Username;
        credential.Password = request.Password;

        await _credentials.UpdateCredential(credential);
        await _hub.Clients.All.NotifyCredentialUpdated(credential.Id);

        var response = _mapper.Map<Credential, CredentialResponse>(credential);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCredential(string id)
    {
        var credential = await _credentials.GetCredential(id);
        
        if (credential is null)
            return NotFound();

        await _credentials.DeleteCredential(credential);
        await _hub.Clients.All.NotifyCredentialDeleted(credential.Id);
        
        return NoContent();
    }
}