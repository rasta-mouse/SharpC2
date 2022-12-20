using System;

using ProtoBuf;

namespace Drone.Messages;

[ProtoContract]
public sealed class DroneTask
{
    [ProtoMember(1)]
    public string Id { get; set; }
    
    [ProtoMember(2)]
    public byte Command { get; set; }

    [ProtoMember(3)]
    public string[] Arguments { get; set; } = Array.Empty<string>();

    [ProtoMember(4)]
    public byte[] Artefact { get; set; } = Array.Empty<byte>();
}