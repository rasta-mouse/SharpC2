using System.Text;

namespace SharpC2.Models;

public sealed class DroneCommand
{
    public string Alias { get; set; } = "";
    public byte Command { get; set; } = 0;
    public string Description { get; set; } = "";

    public string Usage
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append($"{Alias} ");

            foreach (var argument in Arguments)
            {
                sb.Append(argument.Optional ? "[" : "<");
                sb.Append(argument.Name);
                sb.Append(argument.Optional ? "] " : "> ");
            }

            return sb.ToString().TrimEnd();
        }
    }
    
    public CommandArgument[] Arguments { get; set; } = Array.Empty<CommandArgument>();

    public sealed class CommandArgument
    {
        public string Name { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool Optional { get; set; } = true;
        public bool Artefact { get; set; } = false;
    }
}