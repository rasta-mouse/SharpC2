using SQLite;

using TeamServer.Tasks;
using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Storage;

[Table("tasks")]
public sealed class TaskRecordDao
{
    [PrimaryKey, Column("task_id")]
    public string TaskId { get; set; }
    
    [Column("drone_id")]
    public string DroneId { get; set; }
    
    [Column("nick")]
    public string Nick { get; set; }
    
    [Column("command")]
    public byte Command { get; set; }
    
    [Column("alias")]
    public string Alias { get; set; }

    [Column("arguments")]
    public string Arguments { get; set; }  // can't store as string[]

    [Column("artefact_path")]
    public string ArtefactPath { get; set; }

    [Column("artefact")]
    public byte[] Artefact { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }
    
    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("status")]
    public int Status { get; set; }
    
    [Column("result_type")]
    public int ResultType { get; set; }
    
    [Column("result")]
    public byte[] Result { get; set; }

    public static implicit operator TaskRecordDao(TaskRecord record)
    {
        return new TaskRecordDao
        {
            TaskId = record.TaskId,
            DroneId = record.DroneId,
            Nick = record.Nick,
            Command = record.Command,
            Alias = record.Alias,
            Arguments = record.Arguments is null ? string.Empty : string.Join("__,__", record.Arguments),
            ArtefactPath = record.ArtefactPath,
            Artefact = record.Artefact,
            StartTime = record.StartTime,
            EndTime = record.EndTime,
            Status = (int)record.Status,
            ResultType = record.ResultType,
            Result = record.Result
        };
    }

    public static implicit operator TaskRecord(TaskRecordDao dao)
    {
        return new TaskRecord
        {
            TaskId = dao.TaskId,
            DroneId = dao.DroneId,
            Nick = dao.Nick,
            Command = dao.Command,
            Alias = dao.Alias,
            Arguments = dao.Arguments.Split("__,__"),
            ArtefactPath = dao.ArtefactPath,
            Artefact = dao.Artefact,
            StartTime = dao.StartTime,
            EndTime = dao.EndTime,
            Status = (TaskStatus)dao.Status,
            ResultType = dao.ResultType,
            Result = dao.Result
        };
    }
}