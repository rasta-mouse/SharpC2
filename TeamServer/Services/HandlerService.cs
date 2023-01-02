using AutoMapper;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public class HandlerService : IHandlerService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    private readonly List<Handler> _handlers = new();

    public HandlerService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task LoadHandlersFromDb()
    {
        var conn = _db.GetAsyncConnection();
        
        var http = await conn.Table<HttpHandlerDao>().ToArrayAsync();
        var tcp = await conn.Table<TcpHandlerDao>().ToArrayAsync();
        var smb = await conn.Table<SmbHandlerDao>().ToArrayAsync();
        var ext = await conn.Table<ExternalHandlerDao>().ToArrayAsync();
        
        foreach (var dao in http)
        {
            var handler = new HttpHandler(dao.Secure)
            {
                Id = dao.Id,
                Name = dao.Name,
                BindPort = dao.BindPort,
                ConnectAddress = dao.ConnectAddress,
                ConnectPort = dao.ConnectPort
            };
                
            _ = handler.Start();
            _handlers.Add(handler);
        }

        foreach (var dao in tcp)
        {
            var handler = new TcpHandler
            {
                Id = dao.Id,
                Name = dao.Name,
                Address = dao.Address,
                Port = dao.Port,
                Loopback = dao.Loopback
            };
            
            _handlers.Add(handler);
        }

        foreach (var dao in smb)
        {
            var handler = new SmbHandler
            {
                Id = dao.Id,
                Name = dao.Name,
                PipeName = dao.PipeName
            };
            
            _handlers.Add(handler);
        }

        foreach (var dao in ext)
        {
            var handler = new ExternalHandler
            {
                Id = dao.Id,
                Name = dao.Name,
                BindPort = dao.BindPort
            };

            _ = handler.Start();
            _handlers.Add(handler);
        }
    }

    public async Task Add(Handler handler)
    {
        // keep running handlers in memory
        _handlers.Add(handler);
        
        // and write to the db
        var conn = _db.GetAsyncConnection();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
            {
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.InsertAsync(httpDao);
                
                break;
            }

            case HandlerType.SMB:
            {
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.InsertAsync(smbDao);
                
                break;
            }

            case HandlerType.TCP:
            {
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.InsertAsync(tcpDao);
                
                break;
            }

            case HandlerType.EXTERNAL:
            {
                var extDao = _mapper.Map<ExternalHandler, ExternalHandlerDao>((ExternalHandler)handler);
                await conn.InsertAsync(extDao);
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public T Get<T>(string name) where T : Handler
    {
        return (T)_handlers.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<T> Get<T>() where T : Handler
    {
        return _handlers
            .Where(h => h.GetType() == typeof(T))
            .Cast<T>();
    }

    public async Task Update(Handler handler)
    {
        var conn = _db.GetAsyncConnection();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
            {
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.UpdateAsync(httpDao);
                
                break;
            }

            case HandlerType.SMB:
            {
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.UpdateAsync(smbDao);
                
                break;
            }

            case HandlerType.TCP:
            {
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.UpdateAsync(tcpDao);
                
                break;
            }
            
            case HandlerType.EXTERNAL:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public async Task Delete(Handler handler)
    {
        // remove from memory
        _handlers.Remove(handler);
        
        // remove from db
        var conn = _db.GetAsyncConnection();

        switch (handler.HandlerType)
        {
            case HandlerType.HTTP:
            {
                var httpDao = _mapper.Map<HttpHandler, HttpHandlerDao>((HttpHandler)handler);
                await conn.DeleteAsync(httpDao);
                
                break;
            }

            case HandlerType.SMB:
            {
                var smbDao = _mapper.Map<SmbHandler, SmbHandlerDao>((SmbHandler)handler);
                await conn.DeleteAsync(smbDao);
                
                break;
            }

            case HandlerType.TCP:
            {
                var tcpDao = _mapper.Map<TcpHandler, TcpHandlerDao>((TcpHandler)handler);
                await conn.DeleteAsync(tcpDao);
                
                break;
            }

            case HandlerType.EXTERNAL:
            {
                var extDao = _mapper.Map<ExternalHandler, ExternalHandlerDao>((ExternalHandler)handler);
                await conn.DeleteAsync(extDao);
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}