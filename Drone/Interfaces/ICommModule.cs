using System.Collections.Generic;
using System.Threading.Tasks;
using Drone.Messages;

namespace Drone.Interfaces;

public interface ICommModule
{
    void Init(Metadata metadata);
    Task<IEnumerable<C2Frame>> ReadFrames();
    Task SendFrames(IEnumerable<C2Frame> frames);
}