using TeamServer.Pivots;

namespace TeamServer.Interfaces;

public interface IReversePortForwardService
{
    // create
    Task Add(ReversePortForward fwd);
    
    // read
    Task<ReversePortForward> Get(string forwardId);
    Task<IEnumerable<ReversePortForward>> GetAll();
    Task<IEnumerable<ReversePortForward>> GetAll(string droneId);

    // delete
    Task Delete(ReversePortForward forward);
    Task Delete(IEnumerable<ReversePortForward> forwards);
}