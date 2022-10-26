using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Services;

public class DroneService : IDroneService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;
    
    private readonly Dictionary<string, HashSet<string>> _adjacencyList = new();

    public DroneService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
        
        // build the adjacency list on first construction
        BuildGraph().GetAwaiter();
    }

    private async Task BuildGraph()
    {
        var drones = await GetDrones();

        foreach (var drone in drones)
        {
            AddVertex(drone.Metadata.Id);
            
            if (!string.IsNullOrWhiteSpace(drone.Parent))
                AddEdge(drone.Parent, drone.Metadata.Id);
        }
    }

    public async Task AddDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.InsertAsync(dao);
    }

    public async Task<Drone> GetDrone(string id)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().FirstOrDefaultAsync(d => d.Id.Equals(id));
        var drone = _mapper.Map<DroneDao, Drone>(dao);

        return drone;
    }

    public async Task<IEnumerable<Drone>> GetDrones()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<DroneDao>().ToArrayAsync();
        var drones = _mapper.Map<IEnumerable<DroneDao>, IEnumerable<Drone>>(dao);

        return drones;
    }

    public async Task UpdateDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.UpdateAsync(dao);
    }

    public async Task DeleteDrone(Drone drone)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Drone, DroneDao>(drone);
        
        await conn.DeleteAsync(dao);
    }

    public void AddVertex(string drone)
    {
        if (!_adjacencyList.ContainsKey(drone))
            _adjacencyList.Add(drone, new HashSet<string>());
    }

    public void DeleteVertex(string drone)
    {
        if (_adjacencyList.ContainsKey(drone))
            _adjacencyList.Remove(drone);
    }

    public void AddEdge(string start, string end)
    {
        if (_adjacencyList.TryGetValue(start, out var edges))
            edges.Add(end);
    }

    public void DeleteEdge(string start, string end)
    {
        if (_adjacencyList.TryGetValue(start, out var edges))
            edges.Remove(end);
    }

    public bool PathExists(string start, string end)
    {
        var visited = new HashSet<string>();

        if (!_adjacencyList.ContainsKey(start))
            return false;

        var stack = new Stack<string>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var vertex = stack.Pop();

            if (visited.Contains(vertex))
                continue;

            if (vertex.Equals(end))
                return true;

            visited.Add(vertex);

            foreach (var neighbour in _adjacencyList[vertex])
                if (!visited.Contains(neighbour))
                    stack.Push(neighbour);
        }

        return false;
    }

    public IEnumerable<string> FindPath(string start, string end)
    {
        var map = new Dictionary<string, string>();

        var queue = new Stack<string>();
        queue.Push(start);

        while (queue.Count > 0)
        {
            var vertex = queue.Pop();

            foreach (var neighbour in _adjacencyList[vertex])
            {
                if (map.ContainsKey(neighbour))
                    continue;

                map[neighbour] = vertex;
                queue.Push(neighbour);
            }
        }

        var path = new List<string>();
        var current = end;

        while (!current.Equals(start))
        {
            path.Add(current);
            current = map[current];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    public IEnumerable<string> DepthFirstSearch(string start)
    {
        var visited = new HashSet<string>();

        if (!_adjacencyList.ContainsKey(start))
            return visited;

        var stack = new Stack<string>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var vertex = stack.Pop();

            if (visited.Contains(vertex))
                continue;

            visited.Add(vertex);

            if (!_adjacencyList.ContainsKey(vertex))
                continue;
            
            foreach (var neighbour in _adjacencyList[vertex])
                if (!visited.Contains(neighbour))
                    stack.Push(neighbour);
        }

        return visited;
    }
}