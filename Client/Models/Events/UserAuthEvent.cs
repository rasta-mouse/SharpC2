namespace Client.Models.Events;

public sealed class UserAuthEvent
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Nick { get; set; }
    public bool Result { get; set; }
}