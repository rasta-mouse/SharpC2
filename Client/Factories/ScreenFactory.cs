using PrettyPrompt;
using PrettyPrompt.Highlighting;

using SharpC2.Interfaces;
using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.Factories;

public class ScreenFactory : IScreenFactory
{
    private readonly IApiService _api;
    private readonly IHubService _hub;

    public ScreenFactory(IApiService api, IHubService hub)
    {
        _api = api;
        _hub = hub;
    }

    public T GetScreen<T>() where T : Screen
    {
        if (typeof(T) == typeof(LoginScreen))
            return GetLoginScreen() as T;

        if (typeof(T) == typeof(DroneScreen))
            return GetDroneScreen() as T;

        if (typeof(T) == typeof(HandlerScreen))
            return GetListenerScreen() as T;

        if (typeof(T) == typeof(InteractionScreen))
            return GetInteractionScreen() as T;

        throw new NotImplementedException();
    }

    private LoginScreen GetLoginScreen()
    {
        return new LoginScreen(_api, _hub);
    }

    private DroneScreen GetDroneScreen()
    {
        var screen = new DroneScreen(_api, _hub, this);

        var config = new PromptConfiguration(
            prompt: new FormattedString("drones # ", new FormatSpan(0, 6, AnsiColor.Green)));
        
        screen.Prompt = new Prompt(
            configuration: config,
            callbacks: new DroneScreen.PromptCallbacks());

        return screen;
    }

    private HandlerScreen GetListenerScreen()
    {
        var screen = new HandlerScreen(_api, _hub);
        
        var config = new PromptConfiguration(
            prompt: new FormattedString("handlers # ", new FormatSpan(0, 8, AnsiColor.Green)));
        
        screen.Prompt = new Prompt(
            configuration: config,
            callbacks: new HandlerScreen.PromptCallbacks());

        return screen;
    }

    private InteractionScreen GetInteractionScreen()
    {
        return new InteractionScreen(_api, _hub);
    }
}