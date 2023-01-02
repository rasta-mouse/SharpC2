using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Requests;
using SharpC2.API.Responses;

using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Messages;
using TeamServer.Tasks;
using TeamServer.Utilities;

using TaskStatus = TeamServer.Tasks.TaskStatus;

namespace TeamServer.Controllers;

[Authorize]
[ApiController]
[Route(Routes.V1.Tasks)]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly IDroneService _drones;
    private readonly ICryptoService _crypto;
    private readonly IHubContext<NotificationHub, INotificationHub> _hub;

    public TasksController(ITaskService tasks, IDroneService drones, IHubContext<NotificationHub, INotificationHub> hub,
        ICryptoService crypto)
    {
        _tasks = tasks;
        _drones = drones;
        _hub = hub;
        _crypto = crypto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskRecordResponse>>> GetTaskRecords()
    {
        var tasks = await _tasks.GetAll();
        var response = tasks.Select(t => (TaskRecordResponse)t);

        return Ok(response);
    }

    [HttpGet("{droneId}")]
    public async Task<ActionResult<IEnumerable<TaskRecordResponse>>> GetTaskRecords(string droneId)
    {
        var drone = await _drones.Get(droneId);

        if (drone is null)
            return NotFound();
        
        var tasks = await _tasks.GetAllByDrone(droneId);
        var response = tasks.Select(t => (TaskRecordResponse)t);

        return Ok(response);
    }
    
    [HttpGet("{droneId}/{taskId}")]
    public async Task<ActionResult<TaskRecordResponse>> GetTaskRecord(string droneId, string taskId)
    {
        var drone = await _drones.Get(droneId);

        if (drone is null)
            return NotFound("Drone not found");
        
        var task = await _tasks.Get(taskId);

        if (task is null)
            return NotFound("Task not found");
        
        return Ok((TaskRecordResponse)task);
    }

    [HttpPost("{droneId}")]
    public async Task<ActionResult<TaskRecordResponse>> GetTaskRecord(string droneId, [FromBody] TaskRequest request)
    {
        var record = (TaskRecord)request;
        record.DroneId = droneId;
        record.Nick = HttpContext.GetClaimFromContext();

        await _tasks.Add(record);
        await _hub.Clients.All.DroneTasked(record.DroneId, record.TaskId);

        return Ok((TaskRecordResponse)record);
    }
    
    [HttpDelete("{droneId}/{taskId}")]
    public async Task<IActionResult> DeleteTaskRecord(string droneId, string taskId)
    {
        var drone = await _drones.Get(droneId);

        if (drone is null)
            return NotFound("Drone not found");
        
        var task = await _tasks.Get(taskId);

        if (task is null)
            return NotFound("Task not found");
        
        // if the task is still pending, just delete it
        if (task.Status == TaskStatus.PENDING)
        {
            await _tasks.Delete(task);
            await _hub.Clients.All.TaskDeleted(droneId, taskId);
            
            return NoContent();
        }
        
        // if the task is running, send a cancellation frame
        if (task.Status == TaskStatus.RUNNING)
        {
            var frame = new C2Frame(drone.Metadata.Id, FrameType.TASK_CANCEL, await _crypto.Encrypt(taskId));
            _tasks.CacheFrame(droneId, frame);
            
            return NoContent();
        }

        return BadRequest("Task cannot be cancelled or deleted");
    }
}