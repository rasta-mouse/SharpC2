using TeamServer.Messages;

namespace TeamServer.Modules;

public class TaskOutputModule : ServerModule
{
    public override FrameType FrameType
        => FrameType.TASK_OUTPUT;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        var output = await Crypto.Decrypt<TaskOutput>(frame.Data);
        var record = await Tasks.Get(output.TaskId);
        
        if (record is null)
            return;
        
        record.Update(output);

        await Tasks.Update(record);
        await Hub.Clients.All.TaskUpdated(record.DroneId, record.TaskId);
    }
}