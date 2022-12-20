using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Drone.Commands;

public sealed class ListDirectory : DroneCommand
{
    public override byte Command => 0x1A;
    public override bool Threaded => false;

    public override void Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var path = task.Arguments.Any()
            ? task.Arguments[0]
            : Directory.GetCurrentDirectory();
        
        List<DirectoryEntry> results = new();
        
        results.AddRange(GetDirectoryInfo(path));
        results.AddRange(GetFileInfo(path));

        Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.COMPLETE, results.Serialize()));
    }
    
    private static IEnumerable<DirectoryEntry> GetDirectoryInfo(string path)
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            var info = new DirectoryInfo(directory);
            
            yield return new DirectoryEntry
            {
                Name = info.FullName,
                Length = 0,
                CreationTime = info.CreationTimeUtc,
                LastAccessTime = info.LastAccessTimeUtc,
                LastWriteTime = info.LastWriteTimeUtc
            };
        }
    }

    private static IEnumerable<DirectoryEntry> GetFileInfo(string path)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            var info = new FileInfo(file);
            
            yield return new DirectoryEntry
            {
                Name = info.FullName,
                Length = info.Length,
                CreationTime = info.CreationTimeUtc,
                LastAccessTime = info.LastAccessTimeUtc,
                LastWriteTime = info.LastWriteTimeUtc
            };
        }
    }
}