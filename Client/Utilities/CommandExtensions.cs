using SharpC2.Models;

using Spectre.Console;

namespace SharpC2.Utilities;

public static class CommandExtensions
{
    public static Table FormatTable(this IEnumerable<DroneCommand> commands)
    {
        var table = new Table();

        table.AddColumn("alias");
        table.AddColumn("description");

        foreach (var command in commands)
            table.AddRow(
                command.Alias,
                command.Description);

        return table;
    }
}