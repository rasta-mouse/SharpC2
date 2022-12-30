using TeamServer.Drones;
using TeamServer.Messages;

namespace TeamServer.Interfaces;

public interface IServerService
{
    Task HandleInboundFrame(C2Frame frame);
    Task<IEnumerable<C2Frame>> GetOutboundFrames(Metadata metadata);
}