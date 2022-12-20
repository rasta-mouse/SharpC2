using TeamServer.Drones;

namespace TeamServer.Interfaces;

public interface IDroneService
{
    // create
    Task Add(Drone drone);
    
    // read
    Task<Drone> Get(string id);
    Task<IEnumerable<Drone>> Get();

    // update
    Task Update(Drone drone);

    // delete
    Task Delete(Drone drone);
}