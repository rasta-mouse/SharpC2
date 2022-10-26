using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SharpC2.API;

using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Payloads)]
public sealed class PayloadsController : ControllerBase
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public PayloadsController(IHandlerService handlers, IPayloadService payloads)
    {
        _handlers = handlers;
        _payloads = payloads;
    }

    [HttpGet("{handler}/{format}")]
    public async Task<ActionResult<byte[]>> GeneratePayload(string handler, int format)
    {
        var h = _handlers.GetHandler<Handler>(handler);

        if (h is null)
            return NotFound("Handler not found");

        return await _payloads.GeneratePayload(h, (PayloadFormat)format);
    }
}