namespace Drone;

public static class Program
{
#if DEBUG
    public static void Main(string[] args)
    {
        Execute();
    }
#endif
    
    public static void Execute()
    {
        var drone = new Drone();
        drone.Start();
    }
}