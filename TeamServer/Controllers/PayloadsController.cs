using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;

using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Payloads)]
public class PayloadsController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public PayloadsController(IHandlerService handlers, IPayloadService payloads)
    {
        _handlers = handlers;
        _payloads = payloads;
    }

    [HttpGet]
    public async Task<IActionResult> GeneratePayload([FromQuery] string handler, [FromQuery] int format)
    {
        var h = _handlers.Get<Handler>(handler);
        
        if (h is null)
            return NotFound();

        if (!Enum.IsDefined(typeof(PayloadFormat), format))
            return BadRequest("Unknown format");

        var payload = await _payloads.GeneratePayload(h, (PayloadFormat)format);
        return new FileContentResult(payload, "application/octet-stream");
    }
}