using System.Text;

using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class TaskOutput
{
    [ProtoMember(1)]
    public string TaskId { get; set; }
    
    [ProtoMember(2)]
    public TaskStatus Status { get; set; }
    
    [ProtoMember(3)]
    public byte[] Output { get; set; }

    public TaskOutput()
    {
        
    }

    public TaskOutput(string taskId, TaskStatus status)
    {
        TaskId = taskId;
        Status = status;
    }

    public TaskOutput(string taskId, string output)
    {
        TaskId = taskId;
        Status = TaskStatus.COMPLETE;
        Output = Encoding.UTF8.GetBytes(output);
    }
    
    public TaskOutput(string taskId, TaskStatus status, string output)
    {
        TaskId = taskId;
        Status = status;
        Output = Encoding.UTF8.GetBytes(output);
    }
    
    public TaskOutput(string taskId, TaskStatus status, byte[] output)
    {
        TaskId = taskId;
        Status = status;
        Output = output;
    }
}

public enum TaskStatus
{
    RUNNING = 2,
    COMPLETE = 3,
    ABORTED = 4
}