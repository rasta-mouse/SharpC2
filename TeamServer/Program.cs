using System.Net;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using TeamServer.Filters;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Services;
using TeamServer.Utilities;

namespace TeamServer;

internal static class Program
{
    private const string KeyFile = "server.key";
    private const string CertFile = "server.crt";
    
    private static WebApplication _app;
    
    internal static T GetService<T>()
        => _app.Services.GetRequiredService<T>();
    
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ./teamserver <ip> <password>");
            return;
        }
        
        if (!IPAddress.TryParse(args[0], out var address))
        {
            Console.WriteLine("Invalid IP address.");
            return;
        }
        
        var password = args[1];
        
        // generate https cert
        await Helpers.CreateCertificate(address, KeyFile, CertFile);
        
        // initialize auth service
        var authService = new AuthenticationService();
        
        var jwtKey = Helpers.GeneratePasswordHash(password, out _);
        authService.SetServerPassword(password, jwtKey);
        
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(ConfigureKestrel);
        
        // configure jwt auth
        builder.Services
            .AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
                };
            });
        
        // ensure swagger can auth as well
        builder.Services.AddSwaggerGen(s =>
        {
            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n"
            });
            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers(ConfigureControllers);
        builder.Services.AddSignalR();
        
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IAuthenticationService>(authService);
        builder.Services.AddSingleton<IHandlerService, HandlerService>();
        builder.Services.AddSingleton<ITaskService, TaskService>();
        builder.Services.AddSingleton<IServerService, ServerService>();
        builder.Services.AddSingleton<IPeerToPeerService, PeerToPeerService>();
        
        builder.Services.AddTransient<ICryptoService, CryptoService>();
        builder.Services.AddTransient<IDroneService, DroneService>();
        builder.Services.AddTransient<IPayloadService, PayloadService>();
        builder.Services.AddTransient<IEventService, EventService>();
        builder.Services.AddTransient<IHostedFilesService, HostedFileService>();
        builder.Services.AddTransient<IReversePortForwardService, ReversePortForwardService>();
        
        _app = builder.Build();
        
        // build p2p graph from known drones
        var droneService = GetService<IDroneService>();
        var drones = await droneService.Get();
        
        var p2pService = GetService<IPeerToPeerService>();
        p2pService.InitFromDrones(drones);

        // load saved handlers from DB
        await LoadHandlersFromDatabase();
        
        if (_app.Environment.IsDevelopment())
        {
            _app.UseSwagger();
            _app.UseSwaggerUI();
        }

        _app.UseHttpsRedirection();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        
        _app.MapHub<NotificationHub>("/SharpC2");
        
        await _app.RunAsync();
    }
    
    private static void ConfigureKestrel(KestrelServerOptions kestrel)
    {
        kestrel.Listen(IPAddress.Any, 50050, ListenOptions);
    }
    
    private static void ListenOptions(ListenOptions o)
    {
        o.Protocols = HttpProtocols.Http1AndHttp2;
        o.UseHttps("server.key");
    }
    
    private static void ConfigureControllers(MvcOptions opts)
    {
        opts.Filters.Add<InjectionFilters>();
    }
    
    private static async Task LoadHandlersFromDatabase()
    {
        var service = (HandlerService) GetService<IHandlerService>();
        await service.LoadHandlersFromDb();
    }
}