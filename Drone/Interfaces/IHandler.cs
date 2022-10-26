using System.Collections.Generic;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Interfaces;

public interface IHandler
{
    void Init(Metadata metadata, IConfig config, ICrypto crypto);
    Task Start();
    bool GetInbound(out IEnumerable<C2Message> messages);
    void QueueOutbound(DroneTaskOutput output);
    void Stop();
}