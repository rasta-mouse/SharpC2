namespace Client.Models.Events;

public abstract class SharpC2Event
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    
    public abstract EventType Type { get; }
}

public enum EventType
{
    USER_AUTH,
    WEB_LOG
}