using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.Request;
using SharpC2.API.Response;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;
using TeamServer.Utilities;

namespace TeamServer.Controllers;

[ApiController]
[Authorize]
[Route(Routes.V1.Tasks)]
public sealed class TasksController : ControllerBase
{
    private readonly IDroneService _drones;
    private readonly ITaskService _tasks;
    private readonly IMapper _mapper;
    private readonly IHubContext<HubService, IHubService> _hub;

    public TasksController(IDroneService drones, ITaskService tasks, IMapper mapper, IHubContext<HubService, IHubService> hub)
    {
        _drones = drones;
        _tasks = tasks;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DroneTaskResponse>>> GetAllTasks()
    {
        var tasks = await _tasks.GetAllTasks();
        var response = _mapper.Map<IEnumerable<DroneTaskRecord>, IEnumerable<DroneTaskResponse>>(tasks);

        return Ok(response);
    }

    [HttpGet("{droneId}")]
    public async Task<ActionResult<IEnumerable<DroneTaskResponse>>> GetTasks(string droneId)
    {
        var drone = await _drones.GetDrone(droneId);
        if (drone is null) return NotFound();
        
        var tasks = await _tasks.GetTasks(droneId);
        var response = _mapper.Map<IEnumerable<DroneTaskRecord>, IEnumerable<DroneTaskResponse>>(tasks);

        return Ok(response);
    }
    
    [HttpGet("{droneId}/{taskId}")]
    public async Task<ActionResult<IEnumerable<DroneTaskResponse>>> GetTask(string droneId, string taskId)
    {
        var drone = await _drones.GetDrone(droneId);
        if (drone is null) return NotFound();

        var task = await _tasks.GetTask(taskId);
        var response = _mapper.Map<DroneTaskRecord, DroneTaskResponse>(task);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<DroneTaskResponse>> TaskDrone([FromBody] DroneTaskRequest request)
    {
        var drone = await _drones.GetDrone(request.DroneId);
        
        if (drone is null)
            return NotFound();

        var task = new DroneTaskRecord
        {
            TaskId = Guid.NewGuid().ToShortGuid(),
            DroneId = request.DroneId,
            Command = request.Command,
            Arguments = request.Arguments,
            ArtefactPath = request.ArtefactPath,
            Artefact = request.Artefact,
            Status = DroneTaskStatus.Pending
        };

        // add task
        await _tasks.AddTask(task);
        
        // notify hub
        await _hub.Clients.All.NotifyDroneTasked(request.DroneId, request.Alias,
            request.Arguments, request.ArtefactPath);

        // get the record back
        var record = await _tasks.GetTask(task.TaskId);

        // send response
        var response = _mapper.Map<DroneTaskRecord, DroneTaskResponse>(record);
        return Ok(response);
    }
}