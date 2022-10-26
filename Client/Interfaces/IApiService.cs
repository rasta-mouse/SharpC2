using SharpC2.Models;

namespace SharpC2.Interfaces;

public interface IApiService
{
    Task<string> Login(string host, string nick, string pass);

    Task<IEnumerable<Handler>> GetHandlers();
    Task<IEnumerable<HttpHandler>> GetHttpHandlers();
    Task<IEnumerable<SmbHandler>> GetSmbHandlers();
    Task<IEnumerable<TcpHandler>> GetTcpHandlers();

    Task StartHttpHandler(string name, int bindPort, string connectAddress, int connectPort, bool secure);
    Task StartSmbHandler(string name, string pipeName);
    Task StartTcpHandler(string name, int bindPort, bool localhost);
    Task StopHandler(string name);

    Task<IEnumerable<Drone>> GetDrones();
    Task<Drone> GetDrone(string id);
    Task DeleteDrone(string id);
    Task TaskDrone(string drone, string alias, byte command, string[] arguments, string artefactPath, byte[] artefact);

    Task<DroneTask> GetDroneTask(string droneId, string taskId);

    Task<byte[]> GeneratePayload(string handler, int format);
}