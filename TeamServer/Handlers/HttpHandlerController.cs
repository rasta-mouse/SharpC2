using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public class HttpHandlerController : ControllerBase
{
    private readonly ICryptoService _crypto;
    private readonly IDroneService _drones;
    private readonly IServerService _server;
    private readonly IConfiguration _config;
    private readonly IHubContext<HubService, IHubService> _hub;

    public HttpHandlerController(ICryptoService crypto, IDroneService drones, IConfiguration config,
        IHubContext<HubService, IHubService> hub, IServerService server)
    {
        _crypto = crypto;
        _drones = drones;
        _server = server;
        _config = config;
        _hub = hub;
    }

    public async Task<IActionResult> RouteDrone()
    {
        // read metadata
        var metadata = await ExtractMetadata();

        if (metadata is null)
            return NotFound();

        // try and get drone
        var drone = await GetDrone(metadata);
        
        // if post read inbound messages
        if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            using var ms = new MemoryStream();
            await HttpContext.Request.Body.CopyToAsync(ms);

            var messages = ms.ToArray().Deserialize<IEnumerable<C2Message>>();
            await _server.HandleInboundMessages(messages);
        }

        // get outbound messages
        var outbound = await _server.GetOutboundMessages(drone.Metadata.Id);

        if (!outbound.Any())
            return NoContent();
        
        return new FileContentResult(outbound, "application/octet-stream");
    }

    private async Task<DroneMetadata> ExtractMetadata()
    {
        if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var values))
            return null;
        
        // remove "bearer "
        var b64 = values[0].Remove(0, 7);
        var enc = Convert.FromBase64String(b64);
        var (iv, data, checksum) = enc.FromByteArray();

        return await _crypto.DecryptObject<DroneMetadata>(iv, data, checksum);
    }

    private async Task<Drone> GetDrone(DroneMetadata droneMetadata)
    {
        var drone = await _drones.GetDrone(droneMetadata.Id);

        // if null create a new one
        if (drone is null)
        {
            drone = new Drone(droneMetadata)
            {
                Handler = _config.GetValue<string>("name"),
                ExternalAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            // add to db
            await _drones.AddDrone(drone);
            
            // add a new p2p vertex
            _drones.AddVertex(drone.Metadata.Id);

            // notify hub
            await _hub.Clients.All.NotifyNewDrone(drone.Metadata.Id);
        }
        
        return drone;
    }
}