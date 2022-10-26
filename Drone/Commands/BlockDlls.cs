using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class BlockDlls : DroneCommand
{
    public override byte Command => 0x1C;

    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        if (task.Arguments.Any())
            Drone.Config.Set(Setting.BlockDlls, bool.Parse(task.Arguments[0]));

        var config = Drone.Config.Get<bool>(Setting.BlockDlls);
        await Drone.SendOutput(task, $"BlockDLLs is enabled: {config}");
    }
}