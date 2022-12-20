using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TeamServer.Hubs;

[Authorize]
public class NotificationHub: Hub<INotificationHub>
{
    
}