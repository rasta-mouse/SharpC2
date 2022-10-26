using Microsoft.Extensions.DependencyInjection;

using SharpC2.Factories;
using SharpC2.Interfaces;
using SharpC2.Screens;
using SharpC2.Services;

using Spectre.Console;

namespace SharpC2;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        PrintLogo();

        var services = new ServiceCollection()
            .AddSingleton<IScreenFactory, ScreenFactory>()
            .AddSingleton<IApiService, ApiService>()
            .AddSingleton<IHubService, HubService>()
            .AddAutoMapper(typeof(Program))
            .BuildServiceProvider();

        var factory = services.GetRequiredService<IScreenFactory>();
        await using var login = factory.GetScreen<LoginScreen>();
        
        // blocks until login successful
        await login.ShowAsync();

        await using var drones = factory.GetScreen<DroneScreen>();
        await drones.ShowAsync();
    }

    private static void PrintLogo()
    {
        var figlet = new FigletText("SharpC2");
        AnsiConsole.Write(figlet);
    }
}