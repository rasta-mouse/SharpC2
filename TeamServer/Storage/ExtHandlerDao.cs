using SQLite;

using TeamServer.Handlers;

namespace TeamServer.Storage;

[Table("ext_handlers")]
public sealed class ExtHandlerDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("name")]
    public string Name { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
    
    [Column("payload_type")]
    public int PayloadType { get; set; }

    public static implicit operator ExtHandlerDao(ExtHandler handler)
    {
        return new ExtHandlerDao
        {
            Id = handler.Id,
            Name = handler.Name,
            BindPort = handler.BindPort,
            PayloadType = (int)handler.PayloadType
        };
    }

    public static implicit operator ExtHandler(ExtHandlerDao dao)
    {
        return new ExtHandler
        {
            Id = dao.Id,
            Name = dao.Name,
            BindPort = dao.BindPort,
            PayloadType = (PayloadType)dao.PayloadType
        };
    }
}