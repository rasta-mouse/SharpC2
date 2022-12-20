namespace Client.Models;

public sealed class DroneCommand
{
    public string Alias { get; set; } = "";
    public byte Command { get; set; } = 0;
    public string Description { get; set; } = "";
    public string OpSec { get; set; } = "n/a";
    public int Output { get; set; } = 0;
    
    public CommandArgument[] Arguments { get; set; } = Array.Empty<CommandArgument>();
}

public sealed class CommandArgument
{
    public string Name { get; set; } = "";
    public ArgumentType Type { get; set; } = ArgumentType.STRING;
    public string DefaultValue { get; set; } = "";
    public bool Optional { get; set; } = true;
}

public enum ArgumentType
{
    STRING,
    ARTEFACT,
    HANDLER
}