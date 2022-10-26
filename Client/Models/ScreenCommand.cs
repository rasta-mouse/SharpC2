namespace SharpC2.Models;

public sealed class ScreenCommand
{
    public string Name { get; }
    public string Description { get; }
    public Screen.CommandCallback Callback { get; }

    public ScreenCommand(string name, string description, Screen.CommandCallback callback)
    {
        Name = name;
        Description = description;
        Callback = callback;
    }
}