using AutoMapper;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class HandlerService : IHandlerService
{
    private readonly IMapper _mapper;
    private readonly IDatabaseService _db;

    private readonly List<Handler> _handlers = new();

    public HandlerService(IMapper mapper, IDatabaseService db)
    {
        _mapper = mapper;
        _db = db;
    }

    public void LoadHandlersFromDb()
    {
        var conn = _db.GetConnection();

        var http = conn.Table<HttpHandlerDao>().ToArray();
        var tcp = conn.Table<TcpHandlerDao>().ToArray();
        var smb = conn.Table<SmbHandlerDao>().ToArray();
        
        foreach (var dao in http)
        {
            var handler = new HttpHandler(dao.Name, dao.BindPort, dao.ConnectAddress, dao.ConnectPort, dao.Secure);
            handler.Start();
            _handlers.Add(handler);
        }
        
        foreach (var dao in tcp)
        {
            var handler = new TcpHandler(dao.Name, dao.BindPort, dao.LoopbackOnly);
            _handlers.Add(handler);
        }
        
        foreach (var dao in smb)
        {
            var handler = new SmbHandler(dao.Name, dao.PipeName);
            _handlers.Add(handler);
        }
    }

    public async Task AddHandler(Handler handler)
    {
        _handlers.Add(handler);

        var conn = _db.GetAsyncConnection();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.InsertAsync(httpDao);
                break;

            case HandlerType.TCP:
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.InsertAsync(tcpDao);
                break;
            
            case HandlerType.SMB:
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.InsertAsync(smbDao);
                break;

            case HandlerType.EXTERNAL:
                break;
        }
    }

    public T GetHandler<T>(string name) where T : Handler
    {
        return (T) _handlers.FirstOrDefault(h => h.Name.Equals(name));
    }

    public IEnumerable<T> GetHandlers<T>() where T : Handler
    {
        return _handlers
            .Cast<T>();
    }

    public IEnumerable<T> GetHandlers<T>(PayloadType payloadType) where T : Handler
    {
        return _handlers
            .Where(h => h.PayloadType == payloadType)
            .Cast<T>();
    }

    public IEnumerable<T> GetHandlers<T>(HandlerType handlerType) where T : Handler
    {
        return _handlers
            .Where(h => h.HandlerType == handlerType)
            .Cast<T>();
    }

    public async Task UpdateHandler(Handler handler)
    {
        var conn = _db.GetAsyncConnection();
        
        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.UpdateAsync(httpDao);
                break;

            case HandlerType.TCP:
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.UpdateAsync(tcpDao);
                break;
            
            case HandlerType.SMB:
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.UpdateAsync(smbDao);
                break;

            case HandlerType.EXTERNAL:
                break;
        }
    }

    public async Task DeleteHandler(Handler handler)
    {
        _handlers.Remove(handler);
        
        var conn = _db.GetAsyncConnection();
        
        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.DeleteAsync(httpDao);
                break;

            case HandlerType.TCP:
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.DeleteAsync(tcpDao);
                break;
            
            case HandlerType.SMB:
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.DeleteAsync(smbDao);
                break;

            case HandlerType.EXTERNAL:
                break;
        }
    }
}