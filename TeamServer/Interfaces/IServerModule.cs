using TeamServer.Messages;
using TeamServer.Services;

namespace TeamServer.Interfaces;

public interface IServerModule
{
    void Init(ServerService server);
    Task ProcessFrame(C2Frame frame);
}