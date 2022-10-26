using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Handlers;

public abstract class Handler
{
    public abstract event Func<IEnumerable<C2Message>, Task> OnMessagesReceived; 

    protected Metadata Metadata { get; private set; }
    protected Config Config { get; private set; }

    public void Init(Metadata metadata, Config config)
    {
        Metadata = metadata;
        Config = config;
    }

    public abstract Task Start();
    public abstract Task SendMessages(IEnumerable<C2Message> messages);
    public abstract void Stop();
}

public enum HandlerMode
{
    Client,
    Server
}