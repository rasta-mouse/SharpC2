using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Interfaces;

public interface IServerService
{
    Task HandleInboundMessages(IEnumerable<C2Frame> frames);
    Task<IEnumerable<C2Frame>> GetOutboundFrames(Metadata metadata);
}