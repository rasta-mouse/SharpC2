using AutoMapper;

using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class HostedFileService : IHostedFilesService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    public HostedFileService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task Add(HostedFile file)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<HostedFile, HostedFileDao>(file);

        await conn.InsertAsync(dao);
    }

    public async Task<IEnumerable<HostedFile>> Get()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<HostedFileDao>().ToArrayAsync();

        return _mapper.Map<IEnumerable<HostedFileDao>, IEnumerable<HostedFile>>(dao);
    }

    public async Task<HostedFile> Get(string id)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<HostedFileDao>().FirstOrDefaultAsync(h => h.Id.Equals(id));

        return _mapper.Map<HostedFileDao, HostedFile>(dao);
    }

    public async Task Delete(HostedFile file)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<HostedFile, HostedFileDao>(file);

        await conn.DeleteAsync(dao);
    }
}