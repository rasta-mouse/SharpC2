using ProtoBuf;

using TeamServer.Tasks;

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

    public static implicit operator DroneTask(TaskRecord record)
    {
        return new DroneTask
        {
            TaskId = record.TaskId,
            Command = record.Command,
            Arguments = record.Arguments,
            Artefact = record.Artefact
        };
    }
}