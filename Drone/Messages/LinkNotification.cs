using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class LinkNotification
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public string ParentId { get; set; }
    
    [ProtoMember(3)]
    public string ChildId { get; set; }

    public LinkNotification(string taskId, string parentId)
    {
        TaskId = taskId;
        ParentId = parentId;
    }

    public LinkNotification()
    {
        
    }
}