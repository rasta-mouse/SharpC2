using SharpC2.API.Responses;

namespace Client.Models.Events;

public sealed class WebLogEvent : SharpC2Event
{
    public override EventType Type
        => EventType.WEB_LOG;
    
    public string Method { get; set; }
    public string Uri { get; set; }
    public string UserAgent { get; set; }
    public string SourceAddress { get; set; }
    public int ResponseCode { get; set; }

    public static implicit operator WebLogEvent(WebLogEventResponse response)
    {
        if (response is null)
            return null;

        return new WebLogEvent
        {
            Id = response.Id,
            Date = response.Date,
            Method = response.Method,
            Uri = response.Uri,
            UserAgent = response.UserAgent,
            SourceAddress = response.SourceAddress,
            ResponseCode = response.ResponseCode
        };
    }
}