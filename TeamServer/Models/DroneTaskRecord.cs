namespace TeamServer.Models;

public class DroneTaskRecord
{
    public string TaskId { get; set; }
    public string DroneId { get; set; }
    public byte Command { get; set; }
    public string[] Arguments { get; set; }
    public string ArtefactPath { get; set; }
    public byte[] Artefact { get; set; }
    public DateTime StartTime { get; set; }
    public DroneTaskStatus Status { get; set; }
    public DateTime EndTime { get; set; }
    public byte[] Result { get; set; }
    
    public void UpdateTask(DroneTaskOutput output)
    {
        // update the status
        Status = output.Status;

        // update the result
        // only if there is a result
        if (output.Output is not null && output.Output.Length > 0)
        {
            if (Result is null || Result.Length == 0)
            {
                Result = output.Output;
            }
            else
            {
                var tmp = Result;

                Array.Resize(ref tmp, tmp.Length + output.Output.Length);
                Buffer.BlockCopy(output.Output, 0, tmp, Result.Length, output.Output.Length);

                Result = tmp;
            }
        }

        // set the end time
        if (output.Status is DroneTaskStatus.Aborted or DroneTaskStatus.Complete)
            EndTime = DateTime.UtcNow;
    }
}

public enum DroneTaskStatus
{
    Pending = 0,
    Tasked = 1,
    Running = 2,
    Complete = 3,
    Aborted = 4
}