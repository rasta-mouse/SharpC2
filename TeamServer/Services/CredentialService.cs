using AutoMapper;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Storage;

namespace TeamServer.Services;

public class CredentialService : ICredentialService
{
    private readonly IDatabaseService _db;
    private readonly IMapper _mapper;

    public CredentialService(IDatabaseService db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task AddCredential(Credential credential)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Credential, CredentialDao>(credential);
        
        await conn.InsertAsync(dao);
    }

    public async Task AddCredentials(IEnumerable<Credential> credentials)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<IEnumerable<Credential>, IEnumerable<CredentialDao>>(credentials);
        
        await conn.InsertAllAsync(dao);
    }

    public async Task<IEnumerable<Credential>> GetCredentials()
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<CredentialDao>().ToArrayAsync();
        
        return _mapper.Map<IEnumerable<CredentialDao>, IEnumerable<Credential>>(dao);
    }

    public async Task<Credential> GetCredential(string id)
    {
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<CredentialDao>().FirstOrDefaultAsync(d => d.Id.Equals(id));
        
        return _mapper.Map<CredentialDao, Credential>(dao);
    }

    public async Task UpdateCredential(Credential credential)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Credential, CredentialDao>(credential);
        
        await conn.UpdateAsync(dao);
    }

    public async Task DeleteCredential(Credential credential)
    {
        var conn = _db.GetAsyncConnection();
        var dao = _mapper.Map<Credential, CredentialDao>(credential);
        
        await conn.DeleteAsync(dao);
    }
}