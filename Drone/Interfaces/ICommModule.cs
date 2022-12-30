using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drone.Interfaces;

public interface ICommModule : IDisposable
{
    void Init(Metadata metadata);
    Task<IEnumerable<C2Frame>> ReadFrames();
    Task SendFrame(C2Frame frame);
}