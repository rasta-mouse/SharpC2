using System.Diagnostics;
using System.Text;

namespace SharpC2.Models;

[DebuggerDisplay("{Metadata.Id}")]
public sealed class Drone
{
    public Metadata Metadata { get; set; }
    public string Parent { get; set; }
    public string ExternalAddress { get; set; }
    public string Handler { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public DroneStatus Status { get; set; }

    public string GetLastSeen()
    {
        var timeDiff = DateTime.UtcNow - LastSeen;

        // if less than 1s show in ms
        if (timeDiff.TotalSeconds < 1)
            return $"{Math.Round(timeDiff.TotalMilliseconds)}ms";

        // if less than 1m show in s
        if (timeDiff.TotalMinutes < 1)
            return $"{Math.Round(timeDiff.TotalSeconds)}s";

        // if less than 1h show in m
        if (timeDiff.TotalHours < 1) 
            return $"{Math.Round(timeDiff.TotalMinutes)}m";

        // if less than 1d show in h
        if (timeDiff.TotalDays < 1)
            return $"{Math.Round(timeDiff.TotalHours)}h";

        // else show in d
        return $"{Math.Round(timeDiff.TotalDays)}d";
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Metadata.Id} ({Metadata.Process}:{Metadata.ProcessId}) [[{Status}]]");
        sb.AppendLine($"{Metadata.Identity}@{Metadata.Hostname}");
        
        return sb.ToString();
    }
}

public enum DroneStatus
{
    ALIVE,
    DEAD
}