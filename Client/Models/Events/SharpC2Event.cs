namespace Client.Models.Events;

public abstract class SharpC2Event
{
    public string Id { get; set; }
    public EventType Type { get; }
    public DateTime Date { get; set; }
}

public enum EventType
{
    USER_AUTH,
    WEB_LOG
}