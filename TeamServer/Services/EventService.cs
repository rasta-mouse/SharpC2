using AutoMapper;

using TeamServer.Events;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class EventService : IEventService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    public EventService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task Add(SharpC2Event ev)
    {
        var conn = _db.GetAsyncConnection();

        switch (ev.Type)
        {
            case EventType.USER_AUTH:
            {
                var dao = _mapper.Map<UserAuthEvent, UserAuthDao>((UserAuthEvent)ev);
                await conn.InsertAsync(dao);

                break;
            }

            case EventType.WEB_LOG:
            {
                var dao = _mapper.Map<WebLogEvent, WebLogDao>((WebLogEvent)ev);
                await conn.InsertAsync(dao);

                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task<IEnumerable<T>> Get<T>() where T : SharpC2Event
    {
        var conn = _db.GetAsyncConnection();

        if (typeof(T) == typeof(UserAuthEvent))
        {
            var dao = await conn.Table<UserAuthDao>().ToArrayAsync();
            return _mapper.Map<IEnumerable<UserAuthDao>, IEnumerable<UserAuthEvent>>(dao).Cast<T>();
        }

        if (typeof(T) == typeof(WebLogEvent))
        {
            var dao = await conn.Table<WebLogDao>().ToArrayAsync();
            return _mapper.Map<IEnumerable<WebLogDao>, IEnumerable<WebLogEvent>>(dao).Cast<T>();
        }

        throw new ArgumentException("Unknown SharpC2Event type");
    }

    public async Task<T> Get<T>(string id) where T : SharpC2Event
    {
        var conn = _db.GetAsyncConnection();

        if (typeof(T) == typeof(UserAuthEvent))
        {
            var dao = await conn.Table<UserAuthDao>().FirstOrDefaultAsync(e => e.Id.Equals(id));
            return _mapper.Map<UserAuthDao, UserAuthEvent>(dao) as T;
        }

        if (typeof(T) == typeof(WebLogEvent))
        {
            var dao = await conn.Table<WebLogDao>().FirstOrDefaultAsync(e => e.Id.Equals(id));
            return _mapper.Map<WebLogDao, WebLogEvent>(dao) as T;
        }
        
        throw new ArgumentException("Unknown SharpC2Event type");
    }
}