using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public class C2Frame
{
    [ProtoMember(1)]
    public byte[] Type { get; init; }
    
    [ProtoMember(2)]
    public byte[] Length { get; init; }
    
    [ProtoMember(3)]
    public byte[] Value { get; init; }

    public FrameType FrameType
        => (FrameType)BitConverter.ToInt32(Type);

    public C2Frame(FrameType type, byte[] data = null)
    {
        Type = BitConverter.GetBytes((int)type);
        Length = BitConverter.GetBytes(data?.Length ?? 0);
        Value = data;
    }

    public C2Frame()
    {
        
    }
}

public enum FrameType
{
    NOP,
    CHECKIN,
    TASK,
    TASK_OUTPUT,
    TASK_CANCEL,
    RPORTFWD,
    EXIT
}