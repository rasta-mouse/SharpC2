using Client.Services;
using Client.Shared;

using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor.Services;

namespace Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(ConfigureFonts);

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif
        
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AuthenticationProvider>());
        
        builder.Services.AddMudServices();
        builder.Services.AddTransient<CommandService>();
        builder.Services.AddSingleton<SharpC2Api>();
        builder.Services.AddSingleton<SharpC2Hub>();

        return builder.Build();
    }

    private static void ConfigureFonts(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
    }
}