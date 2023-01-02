using SQLite;

using TeamServer.Pivots;

namespace TeamServer.Storage;

[Table("rportfwds")]
public sealed class ReversePortForwardDao
{
    [PrimaryKey, Column("id")]
    public string Id { get; set; }
    
    [Column("drone")]
    public string DroneId { get; set; }
    
    [Column("bind_port")]
    public int BindPort { get; set; }
    
    [Column("forward_host")]
    public string ForwardHost { get; set; }
    
    [Column("forward_port")]
    public int ForwardPort { get; set; }

    public static implicit operator ReversePortForwardDao(ReversePortForward fwd)
    {
        return new ReversePortForwardDao
        {
            Id = fwd.Id,
            DroneId = fwd.DroneId,
            BindPort = fwd.BindPort,
            ForwardHost = fwd.ForwardHost,
            ForwardPort = fwd.ForwardPort
        };
    }

    public static implicit operator ReversePortForward(ReversePortForwardDao dao)
    {
        return new ReversePortForward
        {
            Id = dao.Id,
            DroneId = dao.DroneId,
            BindPort = dao.BindPort,
            ForwardHost = dao.ForwardHost,
            ForwardPort = dao.ForwardPort
        };
    }
}