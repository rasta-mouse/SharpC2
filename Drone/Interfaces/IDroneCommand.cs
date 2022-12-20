using System.Threading;
using System.Threading.Tasks;

using Drone.Messages;

namespace Drone.Interfaces;

public interface IDroneCommand
{
    void Init(Drone drone);
    void Execute(DroneTask task, CancellationToken cancellationToken);
}