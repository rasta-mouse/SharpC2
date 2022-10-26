using System.ServiceProcess;

namespace Drone;

internal static class Program
{
    private static void Main()
    {
        var servicesToRun = new ServiceBase[]
        {
            new DroneService()
        };
        
        ServiceBase.Run(servicesToRun);
    }
}