using Microsoft.AspNetCore.SignalR;

using TeamServer.Interfaces;
using TeamServer.Models;
using TeamServer.Services;

namespace TeamServer.Modules;

public abstract class ServerModule
{
    public abstract byte Module { get; }

    protected IDroneService Drones { get; private set; }
    protected ITaskService Tasks { get; private set; }
    protected ICredentialService Credentials { get; private set; }

    protected IHubContext<HubService, IHubService> Hub { get; private set; }

    public void Init(IDroneService drones, ITaskService tasks, ICredentialService credentials,
        IHubContext<HubService, IHubService> hub)
    {
        Drones = drones;
        Tasks = tasks;
        Credentials = credentials;
        Hub = hub;
    }

    public abstract Task Execute(DroneTaskOutput output);
}