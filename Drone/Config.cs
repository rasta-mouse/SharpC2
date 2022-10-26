using System.Collections.Generic;

namespace Drone;

public class Config
{
    private readonly Dictionary<Setting, object> _configs = new();

    public Config()
    {
        Set(Setting.SleepInterval, SleepInterval);
        Set(Setting.SleepJitter, SleepJitter);
        Set(Setting.SpawnTo, SpawnTo);
        Set(Setting.ParentPid, ParentPid);
        Set(Setting.BlockDlls, BlockDlls);
    }

    public void Set(Setting key, object value)
    {
        if (!_configs.ContainsKey(key))
            _configs.Add(key, null);

        _configs[key] = value;
    }

    public T Get<T>(Setting key)
    {
        if (!_configs.ContainsKey(key))
            return default;

        return (T) _configs[key];
    }
    
    private static int SleepInterval => int.Parse("5");
    private static int SleepJitter => int.Parse("0");
    private static string SpawnTo => "C:\\Windows\\System32\\dllhost.exe";
    private static int ParentPid => -1;
    private static bool BlockDlls => false;
}

public enum Setting
{
    SleepInterval,
    SleepJitter,
    SpawnTo,
    ParentPid,
    BlockDlls
}