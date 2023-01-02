using SharpC2.API.Responses;

namespace Client.Models.Events;

public sealed class UserAuthEvent : SharpC2Event
{
    public override EventType Type
        => EventType.USER_AUTH;
    
    public string Nick { get; set; }
    public bool Result { get; set; }

    public static implicit operator UserAuthEvent(UserAuthEventResponse response)
    {
        if (response is null)
            return null;

        return new UserAuthEvent
        {
            Id = response.Id,
            Date = response.Date,
            Nick = response.Nick,
            Result = response.Result
        };
    }
}