namespace TeamServer.Events;

public abstract class SharpC2Event
{
    public abstract EventType Type { get; }
    
    public string Id { get; set; }
    public DateTime Date { get; set; }
}

public enum EventType
{
    USER_AUTH,
    WEB_LOG
}