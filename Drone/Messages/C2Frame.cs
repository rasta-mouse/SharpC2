using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public class C2Frame
{
    [ProtoMember(1)]
    public string DroneId { get; set; }
    
    [ProtoMember(2)]
    public FrameType Type { get; set; }

    [ProtoMember(3)]
    public byte[] Data { get; set; }

    public C2Frame(string droneId, FrameType type, byte[] data = null)
    {
        DroneId = droneId;
        Type = type;
        Data = data;
    }

    public C2Frame()
    {
        
    }
}

public enum FrameType
{
    CHECK_IN,
    TASK,
    TASK_OUTPUT,
    TASK_CANCEL,
    REV_PORT_FWD,
    SOCKS_PROXY,
    LINK,
    UNLINK,
    EXIT
}