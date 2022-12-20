using Microsoft.AspNetCore.SignalR;

using TeamServer.Events;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Utilities;

namespace TeamServer.Middleware;

public class WebLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEventService _events;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public WebLogMiddleware(RequestDelegate next, IEventService events, IHubContext<NotificationHub, INotificationHub> hub)
    {
        _next = next;
        _events = events;
        _hub = hub;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        
        // the response is available here
        // ignore all 200's for /

        if (!context.Request.Path.ToUriComponent().Equals("/") || (context.Response.StatusCode != 200 && context.Response.StatusCode != 204))
        {
            var ev = new WebLogEvent
            {
                Id = Helpers.GenerateShortGuid(),
                Date = DateTime.UtcNow,
                Method = context.Request.Method,
                Uri = context.Request.Path.ToUriComponent(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                SourceAddress = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                ResponseCode = context.Response.StatusCode
            };
        
            await _events.Add(ev);
            await _hub.Clients.All.NewEvent(EventType.WEB_LOG, ev.Id);
        }
    }
}

public static class WebLogMiddlewareExtensions
{
    public static IApplicationBuilder UseWebLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WebLogMiddleware>();
    }
}