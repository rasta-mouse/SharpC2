using TeamServer.Models;

namespace TeamServer.Interfaces;

public interface IServerService
{
    Task HandleInboundMessages(IEnumerable<C2Message> messages);
    Task<byte[]> GetOutboundMessages(string drone);
}