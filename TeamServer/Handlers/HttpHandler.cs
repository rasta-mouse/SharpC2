using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using TeamServer.Interfaces;
using TeamServer.Services;

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

    public HttpHandler(string name, int bindPort, string connectAddress, int connectPort, bool secure)
    {
        Name = name;
        BindPort = bindPort;
        ConnectAddress = connectAddress;
        ConnectPort = connectPort;
        Secure = secure;

        PayloadType = secure ? PayloadType.REVERSE_HTTPS : PayloadType.REVERSE_HTTP;
    }

    public Task Start()
    {
        _tokenSource = new CancellationTokenSource();

        var host = new HostBuilder()
            .ConfigureWebHostDefaults(ConfigureWebHost)
            .Build();

        return host.RunAsync(_tokenSource.Token);
    }

    private void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls(Secure ? $"https://0.0.0.0:{BindPort}" : $"http://0.0.0.0:{BindPort}");
        builder.UseSetting("name", Name);
        builder.Configure(ConfigureApp);
        builder.ConfigureServices(ConfigureServices);
        builder.ConfigureKestrel(ConfigureKestrel);
    }

    private void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
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
        services.AddSingleton<IServerService, ServerService>();
        services.AddTransient<ICryptoService, CryptoService>();
        services.AddTransient<ICredentialService, CredentialService>();

        services.AddSingleton(_ => Program.GetService<IDroneService>());
        services.AddSingleton(_ => Program.GetService<ITaskService>());
        services.AddSingleton(_ => Program.GetService<IDatabaseService>());
        services.AddSingleton(_ => Program.GetService<IHubContext<HubService, IHubService>>());
        
        services.AddControllers();
        services.AddAutoMapper(typeof(Program));
    }
    private  void ConfigureKestrel(KestrelServerOptions kestrel)
    {
        kestrel.AddServerHeader = false;
        kestrel.Listen(IPAddress.Any, BindPort, ListenOptions);
    }

    private static void ListenOptions(ListenOptions o)
    {
        o.Protocols = HttpProtocols.Http1AndHttp2;
        o.UseHttps("server.key");
    }
    public void Stop()
    {
        // if null or already cancelled
        if (_tokenSource is null || _tokenSource.IsCancellationRequested)
            return;

        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}