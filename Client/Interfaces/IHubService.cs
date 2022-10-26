namespace SharpC2.Interfaces;

public interface IHubService
{
    Task Connect(string hostname, string token);
    
    // handlers
    Func<string, Task> HttpHandlerCreated { get; set; }
    Func<string, Task> HttpHandlerDeleted { get; set; }
    Func<string, Task> TcpHandlerCreated { get; set; }
    Func<string, Task> TcpHandlerDeleted { get; set; }
    Func<string, Task> SmbHandlerCreated { get; set; }
    Func<string, Task> SmbHandlerDeleted { get; set; }
    
    // drones
    Func<string, Task> NewDrone { get; set; }
    Func<string, int, Task> DroneStatusChanged { get; set; }

    // tasks
    Func<string, string, string[], string, Task> DroneTasked { get; set; }
    Func<string, int, Task> SentDroneData { get; set; }
    Func<string, string, Task> DroneTaskUpdated { get; set; }
    Func<string, string, Task> DirectoryListing { get; set; }
    Func<string, string, Task> ProcessListing { get; set; }
    Func<string, string, Task> Screenshot { get; set; }
}