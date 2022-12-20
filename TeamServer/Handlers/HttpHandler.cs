using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
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

    public HttpHandler(bool secure)
    {
        Id = Helpers.GenerateShortGuid();
        Secure = secure;
        PayloadType = secure ? PayloadType.REVERSE_HTTPS : PayloadType.REVERSE_HTTP;
    }

    public HttpHandler()
    {
        
    }

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
        services.AddSingleton(Program.GetService<ITaskService>());
        services.AddSingleton(Program.GetService<IServerService>());
        services.AddSingleton(Program.GetService<IEventService>());
        services.AddSingleton(Program.GetService<IHubContext<NotificationHub, INotificationHub>>());
        
        services.AddAutoMapper(typeof(Program));
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
}