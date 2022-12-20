namespace TeamServer.Events;

public sealed class UserAuthEvent : SharpC2Event
{
    public override EventType Type
        => EventType.USER_AUTH;

    public string Nick { get; set; }
    public bool Result { get; set; }
}