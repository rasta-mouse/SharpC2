using TeamServer.Events;

namespace TeamServer.Interfaces;

public interface IEventService
{
    Task Add(SharpC2Event ev);

    Task<IEnumerable<T>> Get<T>() where T : SharpC2Event;
    Task<T> Get<T>(string id) where T : SharpC2Event;
}