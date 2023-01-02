using TeamServer.Events;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class EventService : IEventService
{
    private readonly IDatabaseService _db;

    public EventService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task Add(SharpC2Event ev)
    {
        var conn = _db.GetAsyncConnection();

        switch (ev.Type)
        {
            case EventType.USER_AUTH:
            {
                await conn.InsertAsync((UserAuthDao)ev);
                break;
            }

            case EventType.WEB_LOG:
            {
                await conn.InsertAsync((WebLogDao)ev);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<IEnumerable<UserAuthEvent>> GetAuthEvents()
    {
        var conn = _db.GetAsyncConnection();
        var events = await conn.Table<UserAuthDao>().ToArrayAsync();
        
        return events.Select(e => (UserAuthEvent)e);
    }

    public async Task<UserAuthEvent> GetAuthEvent(string id)
    {
        var conn = _db.GetAsyncConnection();
        return await conn.Table<UserAuthDao>().FirstOrDefaultAsync(e => e.Id.Equals(id));
    }

    public async Task<IEnumerable<WebLogEvent>> GetWebLogEvents()
    {
        var conn = _db.GetAsyncConnection();
        var events = await conn.Table<WebLogDao>().ToArrayAsync();
        
        return events.Select(e => (WebLogEvent)e);
    }

    public async Task<WebLogEvent> GetWebLogEvent(string id)
    {
        var conn = _db.GetAsyncConnection();
        return await conn.Table<WebLogDao>().FirstOrDefaultAsync(e => e.Id.Equals(id));
    }
}