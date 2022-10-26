using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using TeamServer.Filters;
using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer;

public static class Program
{
    private static WebApplication _app;
    
    private const string KeyFile = "server.key";
    private const string CertFile = "server.crt";

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
        await CreateCertificate(address);

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(ConfigureKestrel);

        // initialise auth service
        var authService = new AuthenticationService();
        builder.Services.AddSingleton<IAuthenticationService>(authService);
        
        // set password on auth service
        var jwtKey = GenerateJwtKey(password);
        authService.SetServerPassword(password, jwtKey);
        
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

        builder.Services.AddControllers(ConfigureControllers);
        builder.Services.AddEndpointsApiExplorer();
        
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
        
        // hub
        builder.Services.AddSignalR(ConfigureHub);

        // SharpC2 services
        builder.Services.AddSingleton<IHandlerService, HandlerService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<ITaskService, TaskService>();
        builder.Services.AddSingleton<IDroneService, DroneService>();
        builder.Services.AddTransient<ICryptoService, CryptoService>();
        builder.Services.AddTransient<IPayloadService, PayloadService>();
        builder.Services.AddTransient<ICredentialService, CredentialService>();

        // automapper
        builder.Services.AddAutoMapper(typeof(Program));
        
        _app = builder.Build();
        
        LoadHandlersFromDatabase();

        if (_app.Environment.IsDevelopment())
        {
            _app.UseSwagger();
            _app.UseSwaggerUI();
        }

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.MapHub<HubService>("/SharpC2");
        
        await _app.RunAsync();
    }

    private static void ConfigureKestrel(KestrelServerOptions kestrel)
    {
        kestrel.Listen(IPAddress.Any, 7443, ListenOptions);
    }

    private static void ListenOptions(ListenOptions o)
    {
        o.Protocols = HttpProtocols.Http1AndHttp2;
        o.UseHttps("server.key");
    }

    private static void ConfigureHub(HubOptions hub)
    {
        hub.MaximumReceiveMessageSize = null;
    }

    private static void ConfigureControllers(MvcOptions opts)
    {
        opts.Filters.Add<JumpFilters>();
        opts.Filters.Add<InjectionFilters>();
    }

    private static void LoadHandlersFromDatabase()
    {
        var service = (HandlerService) GetService<IHandlerService>();
        service.LoadHandlersFromDb();
    }

    private static byte[] GenerateJwtKey(string password)
    {
        using var pbkdf = new Rfc2898DeriveBytes(password, 16, 50000, HashAlgorithmName.SHA256);
        return pbkdf.GetBytes(32);
    }

    private static async Task CreateCertificate(IPAddress address)
    {
        var directory = Directory.GetCurrentDirectory();

        // if already exist, print thumbprint and return
        if (File.Exists(Path.Combine(directory, KeyFile)) && File.Exists(Path.Combine(directory, CertFile)))
        {
            var raw = await File.ReadAllBytesAsync(Path.Combine(directory, KeyFile));
            var x509 = new X509Certificate2(raw);
            Console.WriteLine($"Certificate thumbprint: {x509.Thumbprint}");
            
            return;
        }

        // otherwise, create new self-signed cert
        var distinguishedName = $"CN={address}";

        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest(
            new X500DistinguishedName(distinguishedName), rsa,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature,
            false));

        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection {new("1.3.6.1.5.5.7.3.1")}, false));
        
        var subjectAlternativeName = new SubjectAlternativeNameBuilder();
        subjectAlternativeName.AddIpAddress(address);

        request.CertificateExtensions.Add(subjectAlternativeName.Build());
        
        var cert = request.CreateSelfSigned(
            new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
            new DateTimeOffset(DateTime.UtcNow.AddDays(365)));

        Console.WriteLine($"Certificate thumbprint: {cert.Thumbprint}");
        
        await File.WriteAllBytesAsync(
            Path.Combine(directory, KeyFile), 
            cert.Export(X509ContentType.Pkcs12));
        
        await File.WriteAllBytesAsync(
            Path.Combine(directory, CertFile),
            cert.Export(X509ContentType.Cert));
    }
}