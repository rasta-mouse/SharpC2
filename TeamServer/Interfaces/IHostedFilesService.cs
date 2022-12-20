using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IHostedFilesService
{
    Task Add(HostedFile file);

    Task<IEnumerable<HostedFile>> Get();
    Task<HostedFile> Get(string id);

    Task Delete(HostedFile file);
}