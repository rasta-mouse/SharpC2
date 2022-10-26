namespace Drone.Interfaces;

public interface IConfig
{
    void Set(string key, object value);
    T Get<T>(string key);
}