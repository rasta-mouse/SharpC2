using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public sealed class TaskOutput
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public TaskStatus Status { get; set; }
    
    [ProtoMember(3)]
    public byte[] Output { get; set; }
}

public enum TaskStatus
{
    COMPLETE,
    ABORTED
}