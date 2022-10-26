using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IHandlerService
{
    // create
    Task AddHandler(Handler handler);
    
    // read
    T GetHandler<T>(string name) where T : Handler;
    IEnumerable<T> GetHandlers<T>() where T : Handler;
    IEnumerable<T> GetHandlers<T>(PayloadType payloadType) where T : Handler;
    IEnumerable<T> GetHandlers<T>(HandlerType handlerType) where T : Handler;

    // update
    Task UpdateHandler(Handler handler);
    
    // delete
    Task DeleteHandler(Handler handler);
}