using System.Collections.Immutable;

using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Utilities;

using Spectre.Console;

namespace SharpC2.Screens;

public sealed class HandlerScreen : Screen
{
    private static readonly List<ScreenCommand> Commands = new();
    
    public HandlerScreen(IApiService api, IHubService hub) : base(api, hub)
    {
        Commands.Add(new ScreenCommand("list", "Show Handlers", ShowHandlers));
        Commands.Add(new ScreenCommand("start", "Start a new Handler", StartHandler));
        Commands.Add(new ScreenCommand("stop", "Stop a Handler", StopHandler));
        Commands.Add(new ScreenCommand("back", "Back to previous screen", Back));
        
        Hub.HttpHandlerCreated += OnHttpHandlerCreated;
        Hub.HttpHandlerDeleted += OnHttpHandlerDeleted;
        Hub.TcpHandlerCreated += OnTcpHandlerCreated;
        Hub.TcpHandlerDeleted += OnTcpHandlerDeleted;
        Hub.SmbHandlerCreated += OnSmbHandlerCreated;
        Hub.SmbHandlerDeleted += OnSmbHandlerDeleted;
    }

    public override async Task ShowAsync()
    {
        Running = true;
        
        while (Running)
        {
            var result = await Prompt.ReadLineAsync();
            
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Text))
                continue;

            var command = Commands.FirstOrDefault(c => c.Name.Equals(result.Text));

            if (command is null)
            {
                PrintError("Unknown command.");
                continue;
            }

            await command.Callback.Invoke();
        }
    }

    private async Task ShowHandlers()
    {
        var handlers = (await GetHandlers()).ToArray();
        
        if (!handlers.Any())
        {
            PrintWarning("No Handlers");
            return;
        }

        var table = handlers.FormatTable();
        Print(table);
    }

    private async Task<IEnumerable<Handler>> GetHandlers()
    {
        List<Handler> handlers = new();
        
        handlers.AddRange(await Api.GetHttpHandlers());
        handlers.AddRange(await Api.GetSmbHandlers());
        handlers.AddRange(await Api.GetTcpHandlers());

        return handlers;
    }

    private async Task StartHandler()
    {
        var handlerType = AnsiConsole.Prompt(
            new SelectionPrompt<HandlerType>()
                .Title("Handler Type?")
                .AddChoices(Enum.GetValues<HandlerType>()));

        switch (handlerType)
        {
            case HandlerType.HTTP:
                await StartHttpHandler();
                break;
            
            case HandlerType.SMB:
                await StartSmbHandler();
                break;
            
            case HandlerType.TCP:
                await StartTcpHandler();
                break;
            
            case HandlerType.EXTERNAL:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task StartHttpHandler()
    {
        var name = AnsiConsole.Ask<string>("Handler Name: ");
        var bindPort = AnsiConsole.Ask<int>("Bind Port: ");
        var connectAddress = AnsiConsole.Ask<string>("Connect Address: ");
        var connectPort = AnsiConsole.Ask<int>("Connect Port: ");
        var secure = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Secure?")
                .AddChoices(false, true));

        await Api.StartHttpHandler(name, bindPort, connectAddress, connectPort, secure);
    }

    private async Task StartSmbHandler()
    {
        var name = AnsiConsole.Ask<string>("Handler Name: ");
        var pipeName = AnsiConsole.Ask<string>("PipeName: ");

        await Api.StartSmbHandler(name, pipeName);
    }

    private async Task StartTcpHandler()
    {
        var name = AnsiConsole.Ask<string>("Handler Name: ");
        var bindPort = AnsiConsole.Ask<int>("Bind Port: ");
        var localhost = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Loopback Only?")
                .AddChoices(false, true));

        await Api.StartTcpHandler(name, bindPort, localhost);
    }

    private async Task StopHandler()
    {
        var handlers = await GetHandlers();
        
        var handler = AnsiConsole.Prompt(
            new SelectionPrompt<Handler>()
                .Title("Select Handler")
                .AddChoices(handlers));

        await Api.StopHandler(handler.Name);
    }

    private Task Back()
    {
        Running = false;
        return Task.CompletedTask;
    }
    
    private static Task OnSmbHandlerDeleted(string name)
    {
        PrintInfo($"SMB Handler \"{name}\" deleted.");
        return Task.CompletedTask;
    }

    private static Task OnSmbHandlerCreated(string name)
    {
        PrintInfo($"SMB Handler \"{name}\" created.");
        return Task.CompletedTask;
    }

    private static Task OnTcpHandlerDeleted(string name)
    {
        PrintInfo($"TCP Handler \"{name}\" deleted.");
        return Task.CompletedTask;
    }

    private static Task OnTcpHandlerCreated(string name)
    {
        PrintInfo($"TCP Handler \"{name}\" created.");
        return Task.CompletedTask;
    }

    private static Task OnHttpHandlerDeleted(string name)
    {
        PrintInfo($"HTTP Handler \"{name}\" stopped.");
        return Task.CompletedTask;
    }

    private static Task OnHttpHandlerCreated(string name)
    {
        PrintInfo($"HTTP Handler \"{name}\" started.");
        return Task.CompletedTask;
    }

    public override ValueTask DisposeAsync()
    {
        Hub.HttpHandlerCreated -= OnHttpHandlerCreated;
        Hub.HttpHandlerDeleted -= OnHttpHandlerDeleted;
        Hub.TcpHandlerCreated -= OnTcpHandlerCreated;
        Hub.TcpHandlerDeleted -= OnTcpHandlerDeleted;
        Hub.SmbHandlerCreated -= OnSmbHandlerCreated;
        Hub.SmbHandlerDeleted -= OnSmbHandlerDeleted;
        
        Commands.Clear();
        
        return base.DisposeAsync();
    }

    public class PromptCallbacks : PrettyPrompt.PromptCallbacks
    {
        protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                Commands
                    .Select(cmd =>
                    {
                        var displayText = new FormattedString(cmd.Name);
                        var description = new FormattedString(cmd.Description);
                    
                        return new CompletionItem(
                            replacementText: cmd.Name,
                            displayText: displayText,
                            commitCharacterRules: new[] { new CharacterSetModificationRule(CharacterSetModificationKind.Add, new[] { ' ' }.ToImmutableArray()) }.ToImmutableArray(),
                            getExtendedDescription: _ => Task.FromResult(description)
                        );
                    })
                    .ToArray()
            );
        }
    }
}