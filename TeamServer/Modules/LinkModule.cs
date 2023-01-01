using TeamServer.Messages;

namespace TeamServer.Modules;

public sealed class LinkModule : ServerModule
{
    public override FrameType FrameType => FrameType.LINK;
    
    public override async Task ProcessFrame(C2Frame frame)
    {
        var link = await Crypto.Decrypt<LinkNotification>(frame.Data);
        
        // get the child
        var child = await Drones.Get(link.ChildId);

        if (child is not null)
        {
            child.Parent = link.ParentId;
            await Drones.Update(child);
            
            PeerToPeer.AddEdge(link.ParentId, link.ChildId);
        }
    }
}