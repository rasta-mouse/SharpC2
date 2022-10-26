using System.Collections.Immutable;
using System.Text;

using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;

using SharpC2.Factories;
using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Utilities;

using Spectre.Console;

namespace SharpC2.Screens;

public class DroneScreen : Screen
{
    private readonly IScreenFactory _factory;
    
    private static readonly List<ScreenCommand> Commands = new();

    public DroneScreen(IApiService api, IHubService hub, IScreenFactory factory) : base(api, hub)
    {
        _factory = factory;
        
        Commands.Add(new ScreenCommand("list", "Show Drones in a table view", ShowDroneTable));
        Commands.Add(new ScreenCommand("tree", "Show Drones in a tree view", ShowDroneGraph));
        Commands.Add(new ScreenCommand("interact", "Interact with a Drone", DroneInteract));
        Commands.Add(new ScreenCommand("delete", "Remove one or more Drones", DeleteDrone));
        Commands.Add(new ScreenCommand("handlers", "Manage Handlers", ManageHandlers));
        Commands.Add(new ScreenCommand("payloads", "Generate Payloads", GeneratePayloads));
        Commands.Add(new ScreenCommand("exit", "Exit this client", ExitClient));
        
        Hub.NewDrone += OnNewDrone;
        Hub.Screenshot += OnScreenshot;
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

    private async Task ShowDroneTable()
    {
        var drones = (await Api.GetDrones()).ToArray();

        if (!drones.Any())
        {
            PrintWarning("No Drones");
            return;
        }

        var table = drones.FormatTable();
        Print(table);
    }

    private async Task ShowDroneGraph()
    {
        var drones = (await Api.GetDrones()).ToArray();
        
        if (!drones.Any())
        {
            PrintWarning("No Drones");
            return;
        }

        var tree = drones.FormatTree();
        Print(tree);
    }

    private async Task DroneInteract()
    {
        var drones = (await Api.GetDrones()).ToArray();

        var drone = AnsiConsole.Prompt(
            new SelectionPrompt<Drone>()
                .Title("Select Drone")
                .AddChoices(drones));

        await using var screen = _factory.GetScreen<InteractionScreen>();
        screen.Drone = drone;
        
        var config = new PromptConfiguration(
            prompt: new FormattedString($"{drone.Metadata.Id} # ", new FormatSpan(0, 10, AnsiColor.Green)));
        
        screen.Prompt = new Prompt(
            configuration: config,
            callbacks: new InteractionScreen.PromptCallbacks());

        await screen.ShowAsync();
    }

    private async Task DeleteDrone()
    {
        var drones = (await Api.GetDrones()).ToArray();
        
        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<Drone>()
                .Title("Select Drone(s)")
                .AddChoices(drones));

        foreach (var drone in selected)
            await Api.DeleteDrone(drone.Metadata.Id);
    }

    private async Task ManageHandlers()
    {
        await using var screen = _factory.GetScreen<HandlerScreen>();
        await screen.ShowAsync();
    }

    private async Task GeneratePayloads()
    {
        var allHandlers = (await Api.GetHandlers()).ToArray();

        if (!allHandlers.Any())
        {
            PrintWarning("No Handlers");
            return;
        }

        var selectedHandlers = AnsiConsole.Prompt(
            new MultiSelectionPrompt<Handler>()
                .Title("Select Handler(s)")
                .AddChoices(allHandlers));

        var selectedFormats = AnsiConsole.Prompt(
            new MultiSelectionPrompt<PayloadFormat>()
                .Title("Select Handler(s)")
                .AddChoices(Enum.GetValues<PayloadFormat>()));

        var outputDirectory = AnsiConsole.Prompt(new TextPrompt<string>("Output directory: "));

        if (!Directory.Exists(outputDirectory))
        {
            PrintWarning("Output directory does not exist.");
            
            if (!AnsiConsole.Confirm("Create?"))
                return;

            Directory.CreateDirectory(outputDirectory);
        }

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                // Define tasks
                var task = ctx.AddTask("[green]Generating...[/]");

                var totalActions = selectedHandlers.Count + selectedFormats.Count;

                foreach (var handler in selectedHandlers)
                {
                    foreach (var format in selectedFormats)
                    {
                        var sb = new StringBuilder(handler.Name);

                        switch (format)
                        {
                            case PayloadFormat.Exe:
                                sb.Append(".exe");
                                break;

                            case PayloadFormat.Dll:
                                sb.Append(".dll");
                                break;

                            case PayloadFormat.ServiceExe:
                                sb.Append("_svc.exe");
                                break;

                            case PayloadFormat.PowerShell:
                                sb.Append(".ps1");
                                break;

                            case PayloadFormat.Shellcode:
                                sb.Append(".bin");
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var payload = await Api.GeneratePayload(handler.Name, (int)format);
                        var path = Path.Combine(outputDirectory, sb.ToString());
                        await File.WriteAllBytesAsync(path, payload);

                        // Increment
                        task.Increment((double)100 / totalActions);
                    }
                }

                // hacks
                task.Value = 100;
                task.StopTask();
            });
    }

    private Task ExitClient()
    {
        Running = false;
        return Task.CompletedTask;
    }
    
    private async Task OnNewDrone(string droneId)
    {
        var drone = await Api.GetDrone(droneId);
        
        if (drone is null)
            return;
        
        PrintInfo($"New Drone: {drone.Metadata.Identity}@{drone.Metadata.Hostname} ({drone.Metadata.Process}:{drone.Metadata.ProcessId})");
    }
    
    private async Task OnScreenshot(string droneId, string taskId)
    {
        var task = await Api.GetDroneTask(droneId, taskId);
        
        if (task is null)
            return;

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "Loot", droneId);
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{taskId}.png");
        await File.WriteAllBytesAsync(filePath, task.Result);
        
        PrintSuccess($"Screenshot received, saved to {filePath}");
    }

    public override ValueTask DisposeAsync()
    {
        Hub.Screenshot -= OnScreenshot;
        
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