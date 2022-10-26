using System.Threading;
using System.Threading.Tasks;

namespace Drone.Models;

public abstract class DroneCommand
{
    public abstract byte Command { get; }
    
    public virtual bool Blocking
        => true;

    protected Drone Drone { get; private set; }
    
    public virtual void Init(Drone drone)
    {
        Drone = drone;
    }

    public abstract Task Execute(DroneTask task, CancellationToken cancellationToken);
}