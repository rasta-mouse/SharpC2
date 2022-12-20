using System.Threading.Tasks;

namespace Drone;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        await Execute();
    }

    public static async Task Execute()
    {
        var drone = new Drone();
        await drone.Run();
    }
}