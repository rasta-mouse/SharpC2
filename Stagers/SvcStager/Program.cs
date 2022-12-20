using System.ServiceProcess;

namespace Drone;

internal static class Program
{
    public static void Main()
    {
        var services = new ServiceBase[]
        {
            new DroneService()
        };
        
        ServiceBase.Run(services);
    }
}