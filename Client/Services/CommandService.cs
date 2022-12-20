using System.Reflection;
using Client.Models;

using YamlDotNet.Serialization;

namespace Client.Services;

public class CommandService
{
    private readonly List<DroneCommand> _commands = new();
    
    public CommandService()
    {
#if DEBUG
        var directory = @"C:\Tools\SharpC2\Client\bin\Debug\net6.0-windows10.0.19041.0\win10-x64\Commands";
#endif
#if RELEASE
        var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var directory = Path.Combine(root, "Commands");
#endif

        var files = Directory.EnumerateFiles(directory, "*.yaml");
        var deserializer = new Deserializer();
        
        foreach (var file in files)
        {
            var yaml = File.ReadAllText(file);

            try
            {
                var commands = deserializer.Deserialize<IEnumerable<DroneCommand>>(yaml);
                _commands.AddRange(commands);
            }
            catch
            {
                // ignore
            }
        }
    }
    
    public IEnumerable<DroneCommand> Get()
    {
        return _commands.OrderBy(c => c.Alias);
    }

    public DroneCommand Get(string alias)
    {
        return _commands.FirstOrDefault(c => c.Alias == alias);
    }

    public DroneCommand Get(byte command)
    {
        return _commands.FirstOrDefault(c => c.Command == command);
    }
}