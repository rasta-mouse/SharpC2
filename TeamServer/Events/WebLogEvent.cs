using SharpC2.API.Responses;

namespace TeamServer.Events;

public sealed class WebLogEvent : SharpC2Event
{
    public override EventType Type
        => EventType.WEB_LOG;

    public string Method { get; set; }
    public string Uri { get; set; }
    public string UserAgent { get; set; }
    public string SourceAddress { get; set; }
    public int ResponseCode { get; set; }

    public static implicit operator WebLogEventResponse(WebLogEvent ev)
    {
        return new WebLogEventResponse
        {
            Id = ev.Id,
            Date = ev.Date,
            Method = ev.Method,
            Uri = ev.Uri,
            UserAgent = ev.UserAgent,
            SourceAddress = ev.SourceAddress,
            ResponseCode = ev.ResponseCode
        };
    }
}