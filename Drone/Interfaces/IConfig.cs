namespace Drone.Interfaces;

public interface IConfig
{
    T Get<T>(Setting setting);
    void Set(Setting setting, object value);
}

public enum Setting
{
    SLEEP_INTERVAL,
    SLEEP_JITTER,
    SPAWN_TO,
    PARENT_PID,
    BLOCK_DLLS
}