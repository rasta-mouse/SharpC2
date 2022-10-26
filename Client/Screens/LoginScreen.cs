using SharpC2.Interfaces;
using SharpC2.Models;

using Spectre.Console;

namespace SharpC2.Screens;

public sealed class LoginScreen : Screen
{
    public LoginScreen(IApiService api, IHubService hub) : base(api, hub)
    {
    }

    public override async Task ShowAsync()
    {
        var hostname = string.Empty;
        var token = string.Empty;

        do
        {
            hostname = AnsiConsole.Prompt(new TextPrompt<string>("Host: "));
            var nick = AnsiConsole.Prompt(new TextPrompt<string>("Nick: "));
            var pass = AnsiConsole.Prompt(new TextPrompt<string>("Pass: ").Secret());

            try
            {
                token = await Api.Login(hostname, nick, pass);
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLineInterpolated($"[red][[x]][/] {e.Message}");
            }
            
        } while (string.IsNullOrWhiteSpace(token));
        
        // login successful, connect to hub
        await Hub.Connect(hostname, token);
    }
}