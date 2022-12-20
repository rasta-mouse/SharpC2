using ProtoBuf;

namespace Client.Models.Tasks;

[ProtoContract]
public sealed class DirectoryEntry
{
    [ProtoMember(1)]
    public string Name { get; set; }
    
    [ProtoMember(2)]
    public long Length { get; set; }
    
    [ProtoMember(3)]
    public DateTime CreationTime { get; set; }
    
    [ProtoMember(4)]
    public DateTime LastAccessTime { get; set; }
    
    [ProtoMember(5)]
    public DateTime LastWriteTime { get; set; }
}