using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using AutoMapper;

using RestSharp;
using RestSharp.Authenticators;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using SharpC2.Interfaces;
using SharpC2.Models;

using Spectre.Console;

namespace SharpC2.Services;

public class ApiService : IApiService
{
    private readonly IMapper _mapper;
    private static string _thumbprint;
    
    private RestClient _client;

    public ApiService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<string> Login(string host, string nick, string pass)
    {
        _client?.Dispose();

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback;
        
        var client = new HttpClient(handler);

        _client = new RestClient(client);
        _client.Options.BaseUrl = new Uri($"https://{host}:7443");

        var auth = new AuthenticationRequest
        {
            Nick = nick,
            Pass = pass
        };

        var request = new RestRequest(Routes.V1.Auth, Method.Post).AddJsonBody(auth);
        var response = await _client.PostAsync<AuthenticationResponse>(request);

        if (response is null)
            return string.Empty;
        
        _client.Authenticator = new JwtAuthenticator(response.Token);
            
        return response.Token;
    }

    public async Task<IEnumerable<Handler>> GetHandlers()
    {
        var request = new RestRequest($"{Routes.V1.Handlers}");
        var response = await _client.GetAsync<IEnumerable<HandlerResponse>>(request);

        return _mapper.Map<IEnumerable<HandlerResponse>, IEnumerable<Handler>>(response);
    }

    public async Task<IEnumerable<HttpHandler>> GetHttpHandlers()
    {
        var request = new RestRequest($"{Routes.V1.Handlers}/http");
        var response = await _client.GetAsync<IEnumerable<HttpHandlerResponse>>(request);

        return _mapper.Map<IEnumerable<HttpHandlerResponse>, IEnumerable<HttpHandler>>(response);
    }

    public async Task<IEnumerable<SmbHandler>> GetSmbHandlers()
    {
        var request = new RestRequest($"{Routes.V1.Handlers}/smb");
        var response = await _client.GetAsync<IEnumerable<SmbHandlerResponse>>(request);

        return _mapper.Map<IEnumerable<SmbHandlerResponse>, IEnumerable<SmbHandler>>(response);
    }

    public async Task<IEnumerable<TcpHandler>> GetTcpHandlers()
    {
        var request = new RestRequest($"{Routes.V1.Handlers}/tcp");
        var response = await _client.GetAsync<IEnumerable<TcpHandlerResponse>>(request);

        return _mapper.Map<IEnumerable<TcpHandlerResponse>, IEnumerable<TcpHandler>>(response);
    }

    public async Task StartHttpHandler(string name, int bindPort, string connectAddress, int connectPort, bool secure)
    {
        var handlerReq = new CreateHttpHandlerRequest
        {
            Name = name,
            BindPort = bindPort,
            ConnectAddress = connectAddress,
            ConnectPort = connectPort,
            Secure = secure
        };

        var request = new RestRequest($"{Routes.V1.Handlers}/http", Method.Post).AddJsonBody(handlerReq);
        await _client.PostAsync(request);
    }

    public async Task StartSmbHandler(string name, string pipeName)
    {
        var handlerReq = new CreateSmbHandlerRequest
        {
            Name = name,
            PipeName = pipeName
        };
        
        var request = new RestRequest($"{Routes.V1.Handlers}/smb", Method.Post).AddJsonBody(handlerReq);
        await _client.PostAsync(request);
    }

    public async Task StartTcpHandler(string name, int bindPort, bool localhost)
    {
        var handlerReq = new CreateTcpHandlerRequest
        {
            Name = name,
            BindPort = bindPort,
            LoopbackOnly = localhost
        };
        
        var request = new RestRequest($"{Routes.V1.Handlers}/tcp", Method.Post).AddJsonBody(handlerReq);
        await _client.PostAsync(request);
    }

    public async Task StopHandler(string name)
    {
        var request = new RestRequest($"{Routes.V1.Handlers}/{name}", Method.Delete);
        await _client.DeleteAsync(request);
    }

    public async Task<IEnumerable<Drone>> GetDrones()
    {
        var request = new RestRequest(Routes.V1.Drones);
        var response = await _client.GetAsync<IEnumerable<DroneResponse>>(request);

        return _mapper.Map<IEnumerable<DroneResponse>, IEnumerable<Drone>>(response);
    }

    public async Task<Drone> GetDrone(string id)
    {
        var request = new RestRequest($"{Routes.V1.Drones}/{id}");
        var response = await _client.GetAsync<DroneResponse>(request);

        return _mapper.Map<DroneResponse, Drone>(response);
    }

    public async Task DeleteDrone(string id)
    {
        var request = new RestRequest($"{Routes.V1.Drones}/{id}", Method.Delete);
        await _client.DeleteAsync(request);
    }

    public async Task TaskDrone(string drone, string alias, byte command, string[] arguments, string artefactPath, byte[] artefact)
    {
        var taskReq = new DroneTaskRequest
        {
            DroneId = drone,
            Alias = alias,
            Command = command,
            Arguments = arguments,
            ArtefactPath = artefactPath,
            Artefact = artefact
        };

        var request = new RestRequest(Routes.V1.Tasks, Method.Post).AddJsonBody(taskReq);
        await _client.PostAsync(request);
    }

    public async Task<DroneTask> GetDroneTask(string droneId, string taskId)
    {
        var request = new RestRequest($"{Routes.V1.Tasks}/{droneId}/{taskId}");
        var response = await _client.GetAsync<DroneTaskResponse>(request);

        return _mapper.Map<DroneTaskResponse, DroneTask>(response);
    }

    public async Task<byte[]> GeneratePayload(string handler, int format)
    {
        var request = new RestRequest($"{Routes.V1.Payloads}/{handler}/{format}");
        return await _client.GetAsync<byte[]>(request);
    }

    public static bool ServerCertificateCustomValidationCallback(HttpRequestMessage msg, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errs)
    {
        if (string.IsNullOrWhiteSpace(_thumbprint))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Server Certificate");
            AnsiConsole.WriteLine("==================");
            AnsiConsole.WriteLine($"Issuer: {cert.Issuer}");
            AnsiConsole.WriteLine($"Serial: {cert.SerialNumber}");
            AnsiConsole.WriteLine($"Thumbprint: {cert.Thumbprint}");
            AnsiConsole.WriteLine($"Not Before: {cert.NotBefore:R}");
            AnsiConsole.WriteLine($"Not After: {cert.NotAfter:R}");
            AnsiConsole.WriteLine();

            if (!AnsiConsole.Confirm("Accept?"))
                return false;

            _thumbprint = cert.Thumbprint;
            return true;
        }

        return _thumbprint.Equals(cert.Thumbprint);
    }
}