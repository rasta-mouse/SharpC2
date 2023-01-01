using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public sealed class LinkNotification
{
    [ProtoMember(2)]
    public string ParentId { get; set; }
    
    [ProtoMember(3)]
    public string ChildId { get; set; }
}