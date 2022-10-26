using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface IDroneService
{
    // create
    Task AddDrone(Drone drone);
    
    // read
    Task<Drone> GetDrone(string id);
    Task<IEnumerable<Drone>> GetDrones();

    // update
    Task UpdateDrone(Drone drone);

    // delete
    Task DeleteDrone(Drone drone);
    
    // adjacency list
    void AddVertex(string drone);
    void DeleteVertex(string drone);
    void AddEdge(string start, string end);
    void DeleteEdge(string start, string end);
    bool PathExists(string start, string end);
    IEnumerable<string> FindPath(string start, string end);
    IEnumerable<string> DepthFirstSearch(string start);
}