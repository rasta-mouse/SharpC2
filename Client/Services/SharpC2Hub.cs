using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.Services;

public class SharpC2Hub
{
    public Func<string, Task> HttpHandlerCreated { get; set; }
    public Func<string, Task> HttpHandlerDeleted { get; set; }
    public Func<string, Task> TcpHandlerCreated { get; set; }
    public Func<string, Task> TcpHandlerDeleted { get; set; }
    public Func<string, Task> SmbHandlerCreated { get; set; }
    public Func<string, Task> SmbHandlerDeleted { get; set; }
    
    public Func<string, Task> NewDrone { get; set; }
    public Func<string, Task> DroneCheckedIn { get; set; }
    public Func<string, Task> DroneExited { get; set; }
    public Func<string, Task> DroneDeleted { get; set; }
    public Func<string, Task> DroneLost { get; set; }
    
    public Func<string, string, Task> DroneTasked { get; set; }
    public Func<string, string, Task> TaskUpdated { get; set; }
    public Func<string, string, Task> TaskDeleted { get; set; }
    
    public Func<string, Task> HostedFileAdded { get; set; }
    public Func<string, Task> HostedFileDeleted { get; set; }
    
    public Func<string, Task> ReversePortForwardCreated { get; set; }
    public Func<string, Task> ReversePortForwardDeleted { get; set; }
    
    public Func<int, string, Task> NewEvent { get; set; }
    
    private HubConnection _connection;

    public async Task Connect(string server, string token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"https://{server}:50050/SharpC2", opts =>
            {
                opts.AccessTokenProvider = () => Task.FromResult(token);
                opts.HttpMessageHandlerFactory = HttpMessageHandlerFactory;
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync();

        _connection.On<string>("HttpHandlerCreated", OnHttpHandlerCreated);
        _connection.On<string>("HttpHandlerDeleted", OnHttpHandlerDeleted);
        _connection.On<string>("SmbHandlerCreated", OnSmbHandlerCreated);
        _connection.On<string>("SmbHandlerDeleted", OnSmbHandlerDeleted);
        _connection.On<string>("TcpHandlerCreated", OnTcpHandlerCreated);
        _connection.On<string>("TcpHandlerDeleted", OnTcpHandlerDeleted);
        
        _connection.On<string>("NewDrone", OnNewDrone);
        _connection.On<string>("DroneCheckedIn", OnDroneCheckedIn);
        _connection.On<string>("DroneExited", OnDroneExited);
        _connection.On<string>("DroneDeleted", OnDroneDeleted);
        _connection.On<string>("DroneLost", OnDroneLost);
        
        _connection.On<string, string>("DroneTasked", OnDroneTasked);
        _connection.On<string, string>("TaskUpdated", OnTaskUpdated);
        _connection.On<string, string>("TaskDeleted", OnTaskDeleted);

        _connection.On<string>("HostedFileAdded", OnHostedFileAdded);
        _connection.On<string>("HostedFileDeleted", OnHostedFileDeleted);

        _connection.On<int, string>("NewEvent", OnNewEvent);
        
        _connection.On<string>("ReversePortForwardCreated", OnReversePortForwardCreated);
        _connection.On<string>("ReversePortForwardDeleted", OnReversePortForwardDeleted);
    }
    
    private static HttpMessageHandler HttpMessageHandlerFactory(HttpMessageHandler handler)
    {
        if (handler is HttpClientHandler client)
            client.ServerCertificateCustomValidationCallback = ServerCertificateCustomValidationCallback;

        return handler;
    }
    
    private static bool ServerCertificateCustomValidationCallback(HttpRequestMessage msg, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errs)
    {
        return !string.IsNullOrWhiteSpace(SharpC2Api.AcceptedThumbprint)
               && SharpC2Api.AcceptedThumbprint.Equals(cert.Thumbprint);
    }

    private void OnHttpHandlerCreated(string name) => HttpHandlerCreated?.Invoke(name);
    private void OnHttpHandlerDeleted(string name) => HttpHandlerDeleted?.Invoke(name);
    private void OnSmbHandlerCreated(string name) => SmbHandlerCreated?.Invoke(name);
    private void OnSmbHandlerDeleted(string name) => SmbHandlerDeleted?.Invoke(name);
    private void OnTcpHandlerCreated(string name) => TcpHandlerCreated?.Invoke(name);
    private void OnTcpHandlerDeleted(string name) => TcpHandlerDeleted?.Invoke(name);
    
    private void OnNewDrone(string drone) => NewDrone?.Invoke(drone);
    private void OnDroneCheckedIn(string drone) => DroneCheckedIn?.Invoke(drone);
    private void OnDroneExited(string drone) => DroneExited?.Invoke(drone);
    private void OnDroneDeleted(string drone) => DroneDeleted?.Invoke(drone);
    private void OnDroneLost(string drone) => DroneLost?.Invoke(drone);
    
    private void OnDroneTasked(string drone, string task) => DroneTasked?.Invoke(drone, task);
    private void OnTaskUpdated(string drone, string task) => TaskUpdated?.Invoke(drone, task);
    private void OnTaskDeleted(string drone, string task) => TaskDeleted?.Invoke(drone, task);

    private void OnHostedFileAdded(string id) => HostedFileAdded?.Invoke(id);
    private void OnHostedFileDeleted(string id) => HostedFileDeleted?.Invoke(id);

    private void OnNewEvent(int type, string id) => NewEvent?.Invoke(type, id);
    
    private void OnReversePortForwardCreated(string id) => ReversePortForwardCreated?.Invoke(id);
    private void OnReversePortForwardDeleted(string id) => ReversePortForwardDeleted?.Invoke(id);
}