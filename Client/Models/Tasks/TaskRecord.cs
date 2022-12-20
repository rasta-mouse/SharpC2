using System.Text;

namespace Client.Models.Tasks;

public class TaskRecord
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
    public byte[] Result { get; set; }
    public ResultType ResultType { get; set; }

    public string FullCommand
    {
        get
        {
            var sb = new StringBuilder();

            sb.Append($"{Alias} ");

            if (!string.IsNullOrWhiteSpace(ArtefactPath))
                sb.Append($"{ArtefactPath} ");
            
            if (Arguments.Any())
                sb.Append(string.Join(' ', Arguments));

            return sb.ToString().TrimEnd();
        }
    }

    public void Update(TaskRecord record)
    {
        StartTime = record.StartTime;
        EndTime = record.EndTime;
        Status = record.Status;
        Result = record.Result;
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

public enum ResultType
{
    NONE,
    STRING,
    DIRECTORY_LISTING,
    PROCESS_LISTING,
    SCREENSHOT
}