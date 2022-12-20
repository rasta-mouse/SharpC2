using ProtoBuf;

namespace TeamServer.Messages;

[ProtoContract]
public class DroneTask
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public byte Command { get; set; }

    [ProtoMember(3)]
    public string[] Arguments { get; set; }

    [ProtoMember(4)]
    public byte[] Artefact { get; set; }
}