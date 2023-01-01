using System.Collections.Generic;

namespace Drone;

public class Config
{
    private readonly Dictionary<Setting, object> _configs = new();

    public Config()
    {
        // set default values
        Set(Setting.SLEEP_INTERVAL, SleepTime);
        Set(Setting.SLEEP_JITTER, SleepJitter);
        Set(Setting.SPAWN_TO, SpawnTo);
        Set(Setting.PARENT_PID, ParentPid);
        Set(Setting.BLOCK_DLLS, BlockDlls);
    }

    public T Get<T>(Setting setting)
    {
        if (!_configs.ContainsKey(setting))
            return default;

        return (T)_configs[setting];
    }

    public void Set(Setting setting, object value)
    {
        if (!_configs.ContainsKey(setting))
            _configs.Add(setting, null);

        _configs[setting] = value;
    }

    private static int SleepTime => int.Parse("5");
    private static int SleepJitter => int.Parse("0");
    private static string SpawnTo => "C:\\Windows\\System32\\dllhost.exe";
    private static int ParentPid => -1;
    private static bool BlockDlls => false;
}

public enum Setting
{
    SLEEP_INTERVAL,
    SLEEP_JITTER,
    SPAWN_TO,
    PARENT_PID,
    BLOCK_DLLS
}