using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Messages;
using TeamServer.Utilities;

namespace TeamServer.Tasks;

public sealed class TaskRecord
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public string Nick { get; set; }
    public byte Command { get; set; }
    public string Alias { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TaskStatus Status { get; set; }
    public int ResultType { get; set; }
    public byte[] Result { get; set; }

    public void Update(TaskOutput output)
    {
        if (output.Output is not null)
        {
            if (Result is null)
            {
                Result = output.Output;
            }
            else
            {
                var tmp = Result;
            
                Array.Resize(ref tmp, Result.Length + output.Output.Length);
                Buffer.BlockCopy(output.Output, 0, tmp, Result.Length, output.Output.Length);
            
                Result = tmp;
            }
        }

        Status = (TaskStatus)output.Status;
        
        if (Status is TaskStatus.COMPLETE or TaskStatus.ABORTED)
            EndTime = DateTime.UtcNow;
    }

    public static implicit operator TaskRecord(TaskRequest request)
    {
        return new TaskRecord
        {
            TaskId = Helpers.GenerateShortGuid(),
            Command = request.Command,
            Alias = request.Alias,
            Arguments = request.Arguments,
            ArtefactPath = request.ArtefactPath,
            Artefact = request.Artefact,
            Status = TaskStatus.PENDING,
            ResultType = request.ResultType
        };
    }

    public static implicit operator TaskRecordResponse(TaskRecord record)
    {
        return new TaskRecordResponse
        {
            TaskId = record.TaskId,
            DroneId = record.DroneId,
            Nick = record.Nick,
            Command = record.Command,
            Alias = record.Alias,
            Arguments = record.Arguments,
            ArtefactPath = record.ArtefactPath,
            //Artefact = record.Artefact,
            StartTime = record.StartTime,
            EndTime = record.EndTime,
            Status = (int)record.Status,
            ResultType = record.ResultType,
            Result = record.Result
        };
    }
}

public enum TaskStatus
{
    PENDING,
    TASKED,
    RUNNING,
    COMPLETE,
    ABORTED,
}