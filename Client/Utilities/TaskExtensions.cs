using System.Runtime.Serialization;
using SharpC2.Models;

using Spectre.Console;
using YamlDotNet.Core;

namespace SharpC2.Utilities;

public static class TaskExtensions
{
    public static Table FormatTable(this IEnumerable<DirectoryEntry> entries)
    {
        var table = new Table();

        table.AddColumn("path");
        table.AddColumn("length");
        table.AddColumn("created");
        table.AddColumn("accessed");
        table.AddColumn("written");

        foreach (var entry in entries)
        {
            table.AddRow(
                entry.Name,
                entry.Length.ToString(),
                entry.CreationTime.ToString("u"),
                entry.LastAccessTime.ToString("u"),
                entry.LastWriteTime.ToString("u"));
        }

        return table;
    }

    public static Tree FormatTree(this IEnumerable<ProcessEntry> processes, int dronePid)
    {
        processes = processes.OrderBy(p => p.ProcessId).ToArray();

        var tree = new Tree(processes.Count().ToString());

        Dictionary<int, TreeNode> nodes = new();
        List<ProcessEntry> added = new();

        while (added.Count < processes.Count())
        {
            foreach (var process in processes)
            {
                if (added.Contains(process))
                    continue;
                
                // if pid is 0 or has no parent, add to tree root
                if (process.ProcessId == 0 || processes.All(p => p.ProcessId != process.ParentProcessId))
                {
                    if (nodes.ContainsKey(process.ProcessId))
                        continue;
                    
                    var rootNode = tree.AddNode(process.GetFormattedMarkup(dronePid));
                    nodes.Add(process.ProcessId, rootNode);
                    added.Add(process);
                    continue;
                }
                
                // find parent node
                if (!nodes.ContainsKey(process.ParentProcessId))
                    continue;

                var node = nodes[process.ParentProcessId].AddNode(process.GetFormattedMarkup(dronePid));
                
                if (nodes.ContainsKey(process.ProcessId))
                    continue;
                    
                nodes.Add(process.ProcessId, node);
                added.Add(process);
            }
        }

        return tree;
    }

    private static Markup GetFormattedMarkup(this ProcessEntry process, int pid)
    {
        return process.ProcessId == pid
            ? new Markup($"[yellow]{process}[/]")
            : new Markup(process.ToString());
    }
}