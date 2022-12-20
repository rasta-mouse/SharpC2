using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IHandlerService
{
    // create
    Task Add(Handler handler);
    
    // read
    T Get<T>(string name) where T : Handler;
    IEnumerable<T> Get<T>() where T : Handler;

    // update
    Task Update(Handler handler);
    
    // delete
    Task Delete(Handler handler);
}