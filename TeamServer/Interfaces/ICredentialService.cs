using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface ICredentialService
{
    // create
    Task AddCredential(Credential credential);
    Task AddCredentials(IEnumerable<Credential> credentials);
    
    // read
    Task<IEnumerable<Credential>> GetCredentials();
    Task<Credential> GetCredential(string id);
    
    // update
    Task UpdateCredential(Credential credential);
    
    // delete
    Task DeleteCredential(Credential credential);
}