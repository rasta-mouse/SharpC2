using System.Timers;

using SharpC2.API.Responses;

namespace Client.Models.Drones;

public class Drone : IDisposable
{
    public Metadata Metadata { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public string Parent { get; set; }
    public DroneStatus Status { get; set; }
    
    public Action SeenUpdated { get; set; }

    private string _seen;

    public string Seen
    {
        get => _seen;
        private set
        {
            _seen = value;
            SeenUpdated?.Invoke();
        }
    }

    private readonly System.Timers.Timer _timer;

    public Drone()
    {
        _timer = new System.Timers.Timer();
        _timer.Interval = 500;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Elapsed += TimerElapsed;
        _timer.Start();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        var now = DateTime.UtcNow;
        var diff = now - LastSeen;
        
        // if less than 1s, show in ms
        if (diff.TotalSeconds < 1)
            Seen = $"{Math.Round(diff.TotalMilliseconds)}ms";
        
        // if less than 1m, show in s
        else if (diff.TotalMinutes < 1)
            Seen = $"{Math.Round(diff.TotalSeconds)}s";
        
        // if less than 1h, show in m
        else if (diff.TotalHours < 1)
            Seen = $"{Math.Round(diff.TotalMinutes)}m";
        
        // if less than 1d, show in h
        else if (diff.TotalDays < 1)
            Seen = $"{Math.Round(diff.TotalHours)}s";
        
        // else show in d
        else
            Seen = $"{Math.Round(diff.TotalDays)}d";
    }

    public void CheckIn()
    {
        LastSeen = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"[{Metadata.Hostname}] | {Metadata.Identity} | {Metadata.Process} - {Metadata.Arch}";
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }

    public static implicit operator Drone(DroneResponse response)
    {
        if (response is null)
            return null;

        return new Drone
        {
            Metadata = response.Metadata,
            FirstSeen = response.FirstSeen,
            LastSeen = response.LastSeen,
            Status = (DroneStatus)response.Status,
            Parent = response.Parent
        };
    }
}

public enum DroneStatus
{
    ALIVE,
    LOST,
    DEAD
}