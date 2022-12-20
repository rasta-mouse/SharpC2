using System.Threading;

using Drone.Interfaces;

namespace Drone.Commands;

public abstract class DroneCommand : IDroneCommand
{
    public abstract byte Command { get; }
    public abstract bool Threaded { get; }
    
    protected Drone Drone { get; private set; }
    
    public void Init(Drone drone)
    {
        Drone = drone;
    }

    public abstract void Execute(DroneTask task, CancellationToken cancellationToken);
}