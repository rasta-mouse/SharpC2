using TeamServer.Handlers;
using TeamServer.Interfaces;
using TeamServer.Storage;

namespace TeamServer.Services;

public sealed class HostedFileService : IHostedFilesService
{
    private readonly IDatabaseService _db;

    public HostedFileService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task Add(HostedFile file)
    {
        var conn = _db.GetAsyncConnection();
        await conn.InsertAsync((HostedFileDao)file);
    }

    public async Task<IEnumerable<HostedFile>> Get()
    {
        var conn = _db.GetAsyncConnection();
        var files = await conn.Table<HostedFileDao>().ToArrayAsync();

        return files.Select(f => (HostedFile)f);
    }

    public async Task<HostedFile> Get(string id)
    {
        var conn = _db.GetAsyncConnection();
        return await conn.Table<HostedFileDao>().FirstOrDefaultAsync(h => h.Id.Equals(id));
    }

    public async Task Delete(HostedFile file)
    {
        var conn = _db.GetAsyncConnection();
        await conn.DeleteAsync((HostedFileDao)file);
    }
}