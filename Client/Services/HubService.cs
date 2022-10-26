using Microsoft.AspNetCore.SignalR.Client;

using SharpC2.Interfaces;

namespace SharpC2.Services;

public class HubService : IHubService
{
    public Func<string, Task> HttpHandlerCreated { get; set; }
    public Func<string, Task> HttpHandlerDeleted { get; set; }
    public Func<string, Task> TcpHandlerCreated { get; set; }
    public Func<string, Task> TcpHandlerDeleted { get; set; }
    public Func<string, Task> SmbHandlerCreated { get; set; }
    public Func<string, Task> SmbHandlerDeleted { get; set; }
    
    public Func<string, Task> NewDrone { get; set; }
    public Func<string, int, Task> DroneStatusChanged { get; set; }
    
    public Func<string, string, string[], string, Task> DroneTasked { get; set; }
    public Func<string, int, Task> SentDroneData { get; set; }
    public Func<string, string, Task> DroneTaskUpdated { get; set; }
    public Func<string, string, Task> DirectoryListing { get; set; }
    public Func<string, string, Task> ProcessListing { get; set; }
    public Func<string, string, Task> Screenshot { get; set; }

    private HubConnection _connection;
    
    public async Task Connect(string hostname, string token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"https://{hostname}:7443/SharpC2", o =>
            {
                o.AccessTokenProvider = () => Task.FromResult(token);
                o.HttpMessageHandlerFactory = HttpMessageHandlerFactory;
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();

        _connection.On<string>("NotifyHttpHandlerCreated", OnHttpHandlerCreated);
        _connection.On<string>("NotifyHttpHandlerDeleted", OnHttpHandlerDeleted);
        _connection.On<string>("NotifyTcpHandlerCreated", OnTcpHandlerCreated);
        _connection.On<string>("NotifyTcpHandlerDeleted", OnTcpHandlerDeleted);
        _connection.On<string>("NotifySmbHandlerCreated", OnSmbHandlerCreated);
        _connection.On<string>("NotifySmbHandlerDeleted", OnSmbHandlerDeleted);
        
        _connection.On<string>("NotifyNewDrone", OnNewDrone);
        _connection.On<string, int>("NotifyDroneStatusChanged", OnDroneStatusChanged);
        
        _connection.On<string, string, string[], string>("NotifyDroneTasked", OnDroneTasked);
        _connection.On<string, int>("NotifySentDroneData", OnSentDroneData);
        _connection.On<string, string>("NotifyDroneTaskUpdated", OnDroneTaskUpdated);
        _connection.On<string, string>("NotifyDirectoryListing", OnDirectoryListing);
        _connection.On<string, string>("NotifyProcessListing", OnProcessListing);
        _connection.On<string, string>("NotifyScreenshotAdded", OnScreenshot);
    }

    private void OnHttpHandlerCreated(string name) => HttpHandlerCreated?.Invoke(name);
    private void OnHttpHandlerDeleted(string name) => HttpHandlerDeleted?.Invoke(name);
    private void OnTcpHandlerCreated(string name) => TcpHandlerCreated?.Invoke(name);
    private void OnTcpHandlerDeleted(string name) => TcpHandlerDeleted?.Invoke(name);
    private void OnSmbHandlerCreated(string name) => SmbHandlerCreated?.Invoke(name);
    private void OnSmbHandlerDeleted(string name) => SmbHandlerDeleted?.Invoke(name);

    private void OnNewDrone(string droneId) => NewDrone?.Invoke(droneId);
    private void OnDroneStatusChanged(string droneId, int status)
        => DroneStatusChanged?.Invoke(droneId, status);

    private void OnDroneTasked(string drone, string alias, string[] arguments, string artefactPath)
        => DroneTasked?.Invoke(drone, alias, arguments, artefactPath);

    private void OnSentDroneData(string drone, int size)
        => SentDroneData?.Invoke(drone, size);

    private void OnDroneTaskUpdated(string droneId, string taskId)
        => DroneTaskUpdated?.Invoke(droneId, taskId);

    private void OnDirectoryListing(string droneId, string taskId)
        => DirectoryListing?.Invoke(droneId, taskId);

    private void OnProcessListing(string droneId, string taskId)
        => ProcessListing?.Invoke(droneId, taskId);

    private void OnScreenshot(string droneId, string taskId)
        => Screenshot?.Invoke(droneId, taskId);

    private static HttpMessageHandler HttpMessageHandlerFactory(HttpMessageHandler handler)
    {
        if (handler is HttpClientHandler client)
            client.ServerCertificateCustomValidationCallback = ApiService.ServerCertificateCustomValidationCallback;

        return handler;
    }
}