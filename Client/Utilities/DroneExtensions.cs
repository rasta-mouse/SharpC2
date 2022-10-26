using SharpC2.Models;

using Spectre.Console;

namespace SharpC2.Utilities;

public static class DroneExtensions
{
    public static Table FormatTable(this IEnumerable<Drone> drones)
    {
        var table = new Table();

        table.AddColumn("id");
        table.AddColumn("handler");
        table.AddColumn("ext_ip");
        table.AddColumn("int_ip");
        table.AddColumn("identity");
        table.AddColumn("hostname");
        table.AddColumn("process");
        table.AddColumn("pid");
        table.AddColumn("integrity");
        table.AddColumn("arch");
        table.AddColumn("last");
        table.AddColumn("status");

        foreach (var drone in drones)
        {
            table.AddRow(
                drone.Metadata.Id,
                string.IsNullOrWhiteSpace(drone.Handler) ? "-" : drone.Handler,
                string.IsNullOrWhiteSpace(drone.ExternalAddress) ? "-" : drone.ExternalAddress,
                drone.Metadata.Address,
                drone.Metadata.Identity,
                drone.Metadata.Hostname,
                drone.Metadata.Process,
                drone.Metadata.ProcessId.ToString(),
                drone.Metadata.Integrity.ToString(),
                drone.Metadata.Architecture.ToString(),
                drone.GetLastSeen(),
                drone.Status.ToString());
        }

        return table;
    }

    public static Tree FormatTree(this IEnumerable<Drone> drones)
    {
        drones = drones.ToArray();
        
        var tree = new Tree(drones.Count().ToString());
        
        Dictionary<string, TreeNode> nodes = new();
        List<Drone> added = new();

        while (added.Count < drones.Count())
        {
            foreach (var drone in drones)
            {
                if (added.Contains(drone))
                    continue;
                
                // if drone doesn't have a parent, add it to the tree root
                if (string.IsNullOrWhiteSpace(drone.Parent))
                {
                    var rootNode = tree.AddNode(drone.ToString());
                    nodes.Add(drone.Metadata.Id, rootNode);
                    added.Add(drone);
                    continue;
                }
                
                // find parent drone
                if (!nodes.ContainsKey(drone.Parent))
                    continue;

                var node = nodes[drone.Parent].AddNode(drone.ToString());
                
                if (nodes.ContainsKey(drone.Metadata.Id))
                    continue;
                
                nodes.Add(drone.Metadata.Id, node);
                added.Add(drone);
            }
        }

        return tree;
    }
}