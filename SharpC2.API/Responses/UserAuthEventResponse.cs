namespace SharpC2.API.Responses;

public sealed class UserAuthEventResponse
{
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Nick { get; set; }
    public bool Result { get; set; }
}