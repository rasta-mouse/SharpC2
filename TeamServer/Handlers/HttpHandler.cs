using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;

using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Middleware;
using TeamServer.Utilities;

namespace TeamServer.Handlers;

public class HttpHandler : Handler
{
    private CancellationTokenSource _tokenSource;

    public int BindPort { get; set; }
    public string ConnectAddress { get; set; }
    public int ConnectPort { get; set; }
    public bool Secure { get; set; }

    public override HandlerType HandlerType
        => HandlerType.HTTP;

    public string FilePath { get; set; }

    private void CreateDataDirectory()
    {
        FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", Id);
        
        if (!Directory.Exists(FilePath))
            Directory.CreateDirectory(FilePath);
    }

    public Task Start()
    {
        // ensure data directory exists
        CreateDataDirectory();
        
        _tokenSource = new CancellationTokenSource();

        var host = new HostBuilder()
            .ConfigureWebHostDefaults(ConfigureWebHost)
            .Build();

        return host.RunAsync(_tokenSource.Token);
    }

    private void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls($"http://0.0.0.0:{BindPort}");
        builder.UseSetting("name", Name);
        builder.Configure(ConfigureApp);
        builder.ConfigureServices(ConfigureServices);
        builder.ConfigureKestrel(ConfigureKestrel);
    }

    private void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseWebLogMiddleware();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(FilePath),
            ServeUnknownFileTypes = true,
            DefaultContentType = "test/plain"
        });
        app.UseEndpoints(ConfigureEndpoints);
    }

    private void ConfigureEndpoints(IEndpointRouteBuilder endpoint)
    {
        endpoint.MapControllerRoute("/", "/", new
        {
            controller = "HttpHandler", action = "RouteDrone"
        });
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services.AddSingleton(Program.GetService<IDatabaseService>());
        services.AddSingleton(Program.GetService<ICryptoService>());
        services.AddSingleton(Program.GetService<IDroneService>());
        services.AddSingleton(Program.GetService<IPeerToPeerService>());
        services.AddSingleton(Program.GetService<ITaskService>());
        services.AddSingleton(Program.GetService<IServerService>());
        services.AddSingleton(Program.GetService<IEventService>());
        services.AddSingleton(Program.GetService<IHubContext<NotificationHub, INotificationHub>>());
    }

    private static void ConfigureKestrel(KestrelServerOptions kestrel)
    {
        kestrel.AddServerHeader = false;
    }

    public void Stop()
    {
        if (_tokenSource is null || _tokenSource.IsCancellationRequested)
            return;

        _tokenSource.Cancel();
        _tokenSource.Dispose();
        
        // delete hosted files
        Directory.Delete(FilePath, true);
    }
    
    public static implicit operator HttpHandler(HttpHandlerRequest request)
    {
        return new HttpHandler
        {
            Id = Helpers.GenerateShortGuid(),
            Name = request.Name,
            BindPort = request.BindPort,
            ConnectAddress = request.ConnectAddress,
            ConnectPort = request.ConnectPort,
            Secure = request.Secure,
            PayloadType = request.Secure ? PayloadType.REVERSE_HTTPS : PayloadType.REVERSE_HTTP
        };
    }

    public static implicit operator HttpHandlerResponse(HttpHandler handler)
    {
        return new HttpHandlerResponse
        {
            Id = handler.Id,
            Name = handler.Name,
            BindPort = handler.BindPort,
            ConnectAddress = handler.ConnectAddress,
            ConnectPort = handler.ConnectPort,
            Secure = handler.Secure,
            HandlerType = (int)handler.HandlerType,
            PayloadType = (int)handler.PayloadType,
        };
    }
}