using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;

namespace TeamServer.Services;

[Authorize]
public class HubService : Hub<IHubService>
{
    
}