using System.Collections.Immutable;
using System.Text;

using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Utilities;

using Spectre.Console;

using YamlDotNet.Serialization;

namespace SharpC2.Screens;

public class InteractionScreen : Screen
{
    public Drone Drone { get; set; }
    
    private static List<DroneCommand> _commands = new();
    
    public InteractionScreen(IApiService api, IHubService hub) : base(api, hub)
    {
        LoadDroneCommands();
        
        Hub.DroneStatusChanged += OnDroneStatusChanged;
        Hub.DroneTasked += OnDroneTasked;
        Hub.SentDroneData += OnSentDroneData;
        Hub.DroneTaskUpdated += OnDroneTaskUpdated;
        Hub.DirectoryListing += OnDirectoryListing;
        Hub.ProcessListing += OnProcessListing;
    }

    private static void LoadDroneCommands()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Commands");
        var files = Directory.GetFiles(path, "*.yaml");

        var serializer = new Deserializer();

        foreach (var file in files)
        {
            var yaml = File.ReadAllText(file);

            try
            {
                var command = serializer.Deserialize<IEnumerable<DroneCommand>>(yaml);
                _commands.AddRange(command);
            }
            catch
            {
                PrintWarning($"Deserialization error in {file}");
            }
        }

        _commands = _commands.OrderBy(c => c.Alias).ToList();
    }

    public override async Task ShowAsync()
    {
        Running = true;
        
        while (Running)
        {
            var result = await Prompt.ReadLineAsync();
            
            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Text))
                continue;

            if (result.Text.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                var split = result.Text.Split(' ');
                
                if (split.Length == 1) ShowCommandHelp();
                else ShowCommandHelp(split[1]);
                
                continue;
            }

            if (result.Text.Equals("back", StringComparison.OrdinalIgnoreCase))
            {
                Running = false;
                break;
            }

            var command = _commands.FirstOrDefault(c => c.Alias.Equals(result.Text));

            if (command is null)
            {
                PrintError("Unknown command.");
                continue;
            }

            await HandleDroneCommand(command);
        }
    }

    private static void ShowCommandHelp(string command = "")
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            // list all commands
            var table = _commands.FormatTable();
            Print(table);
            return;
        }

        var droneCommand = _commands.FirstOrDefault(c => c.Alias.Equals(command));

        if (droneCommand is null)
        {
            PrintError("Unknown command");
            return;
        }
        
        Print(new Markup($"{droneCommand.Usage}{Environment.NewLine}".EscapeMarkup()));
    }

    private async Task HandleDroneCommand(DroneCommand command)
    {
        List<string> arguments = new();
        var artefactPath = string.Empty;
        var artefact = Array.Empty<byte>();

        foreach (var argument in command.Arguments)
        {
            if (!string.IsNullOrWhiteSpace(argument.DefaultValue))
            {
                if (argument.Artefact)
                {
                    artefactPath = argument.DefaultValue;
                    
                    if (!File.Exists(artefactPath))
                    {
                        PrintError($"{artefactPath} does not exist");
                        return;
                    }

                    artefact = await File.ReadAllBytesAsync(artefactPath);
                    continue;
                }
                
                arguments.Add(argument.DefaultValue);
                continue;
            }

            // if it's a handler
            if (argument.Name.Equals("handler", StringComparison.OrdinalIgnoreCase))
            {
                var handlers = await Api.GetHandlers();
                
                var handler = AnsiConsole.Prompt(
                    new SelectionPrompt<Handler>()
                        .Title("Select Handler")
                        .AddChoices(handlers));
                
                arguments.Add(handler.Name);
                continue;
            }

            var prompt = new TextPrompt<string>($"{argument.Name}: ");

            if (argument.Optional)
                prompt.AllowEmpty();
            
            var input = AnsiConsole.Prompt(prompt);

            if (!argument.Artefact)
            {
                arguments.Add(input);
                continue;
            }

            artefactPath = input;
            
            if (!File.Exists(artefactPath))
            {
                PrintError($"{artefactPath} does not exist");
                return;
            }

            artefact = await File.ReadAllBytesAsync(artefactPath);
        }

        await Api.TaskDrone(Drone.Metadata.Id, command.Alias, command.Command, arguments.ToArray(), artefactPath, artefact);
    }
    
    private Task OnDroneStatusChanged(string drone, int status)
    {
        if (!Drone.Metadata.Id.Equals(drone))
            return Task.CompletedTask;

        var droneStatus = (DroneStatus)status;

        switch (droneStatus)
        {
            case DroneStatus.ALIVE:
                PrintSuccess("Drone is alive.");
                break;
            
            case DroneStatus.DEAD:
                PrintWarning("Drone is dead.");
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return Task.CompletedTask;
    }

    private Task OnDroneTasked(string drone, string alias, string[] arguments, string artefactPath)
    {
        if (!Drone.Metadata.Id.Equals(drone))
            return Task.CompletedTask;

        var sb = new StringBuilder("Tasked Drone to execute: ");

        if (!string.IsNullOrWhiteSpace(artefactPath))
            sb.Append($"{artefactPath} ");

        if (arguments.Any())
            sb.Append(string.Join(" ", arguments));

        PrintInfo(sb.ToString().TrimEnd());
        return Task.CompletedTask;
    }

    private Task OnSentDroneData(string drone, int size)
    {
        if (!Drone.Metadata.Id.Equals(drone))
            return Task.CompletedTask;
        
        PrintInfo($"Drone checked in, sent {size} bytes");
        return Task.CompletedTask;
    }

    private async Task OnDroneTaskUpdated(string droneId, string taskId)
    {
        if (!Drone.Metadata.Id.Equals(droneId))
            return;

        var task = await Api.GetDroneTask(droneId, taskId);

        if (task?.Result is null || task.Result.Length == 0)
            return;

        if (task.Status == DroneTaskStatus.Aborted) PrintError($"Error received:{Environment.NewLine}");
        else PrintSuccess($"Output received:{Environment.NewLine}");

        Print(new Markup(Encoding.UTF8.GetString(task.Result).EscapeMarkup()));
    }
    
    private async Task OnDirectoryListing(string droneId, string taskId)
    {
        if (!Drone.Metadata.Id.Equals(droneId))
            return;
        
        var task = await Api.GetDroneTask(droneId, taskId);

        if (task?.Result is null || task.Result.Length == 0)
            return;

        var listing = task.Result.Deserialize<IEnumerable<DirectoryEntry>>();
        var table = listing.FormatTable();
        
        PrintSuccess($"Directory listing:{Environment.NewLine}");
        Print(table);
    }
    
    private async Task OnProcessListing(string droneId, string taskId)
    {
        if (!Drone.Metadata.Id.Equals(droneId))
            return;
        
        var task = await Api.GetDroneTask(droneId, taskId);

        if (task?.Result is null || task.Result.Length == 0)
            return;
        
        var processes = task.Result.Deserialize<IEnumerable<ProcessEntry>>();
        var tree = processes.FormatTree(Drone.Metadata.ProcessId);

        PrintSuccess($"Process listing:{Environment.NewLine}");
        Print(tree);
    }

    public override ValueTask DisposeAsync()
    {
        Hub.DroneStatusChanged -= OnDroneStatusChanged;
        Hub.DroneTasked -= OnDroneTasked;
        Hub.SentDroneData -= OnSentDroneData;
        Hub.DroneTaskUpdated -= OnDroneTaskUpdated;
        Hub.DirectoryListing -= OnDirectoryListing;
        Hub.ProcessListing -= OnProcessListing;
        
        _commands.Clear();
        
        return base.DisposeAsync();
    }

    public class PromptCallbacks : PrettyPrompt.PromptCallbacks
    {
        protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                _commands
                    .Select(cmd =>
                    {
                        var displayText = new FormattedString(cmd.Alias);
                        var description = new FormattedString(cmd.Description);
                    
                        return new CompletionItem(
                            replacementText: cmd.Alias,
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