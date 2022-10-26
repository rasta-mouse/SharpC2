using PrettyPrompt;

using SharpC2.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace SharpC2.Models;

public abstract class Screen : IAsyncDisposable
{
    public delegate Task CommandCallback();
    
    public string Name { get; set; }
    public Prompt Prompt { get; set; }
    
    protected bool Running { get; set; }
    
    protected IApiService Api { get; }
    protected IHubService Hub { get; }

    protected Screen(IApiService api, IHubService hub)
    {
        Api = api;
        Hub = hub;
    }

    public abstract Task ShowAsync();

    protected static void PrintInfo(string info)
    {
        AnsiConsole.MarkupLine($"[blue][[+]][/] {info}");
    }

    protected static void PrintSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green][[+]][/] {message}");
    }

    protected static void PrintError(string error)
    {
        AnsiConsole.MarkupLine($"[red][[x]][/] {error}");
    }

    protected static void PrintWarning(string warning)
    {
        AnsiConsole.MarkupLine($"[yellow][[!]][/] {warning}");
    }
    
    protected static void Print(IRenderable item)
    {
        AnsiConsole.Write(item);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (Prompt is not null)
            await Prompt.DisposeAsync();
        
        GC.SuppressFinalize(this);
    }
}