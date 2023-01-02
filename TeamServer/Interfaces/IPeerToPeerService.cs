using TeamServer.Drones;

namespace TeamServer.Interfaces;

public interface IPeerToPeerService
{
    void InitFromDrones(IEnumerable<Drone> drones);
    
    void AddVertex(string vertex);
    void RemoveVertex(string vertex);
    
    void AddEdge(string start, string end);
    void RemoveEdge(string start, string end);
    
    public IEnumerable<string> DepthFirstSearch(string start);
    bool PathExists(string start, string end);
    IEnumerable<string> FindPath(string start, string end);
}