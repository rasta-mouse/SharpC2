namespace Client.Models.Handlers;

public abstract class Handler
{
    public string Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return Name;
    }
}