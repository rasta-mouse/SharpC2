using System;

namespace Drone.Utilities;

public static class Helpers
{
    public static string GenerateShortGuid()
    {
        return Guid.NewGuid()
            .ToString()
            .Replace("-", "")
            .Substring(0, 10);
    }
    
    public static TimeSpan CalculateSleepTime(int interval, int jitter)
    {
        var diff = (int)Math.Round((double)interval / 100 * jitter);

        var min = interval - diff;
        var max = interval + diff;

        var rand = new Random();
        return new TimeSpan(0, 0, rand.Next(min, max));
    }
}