using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Drone;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var bytes = await GetEmbeddedResource("drone");
        var asm = Assembly.Load(bytes);
        
        var task = (Task)asm.GetType("Drone.Program")!.GetMethod("Execute")!.Invoke(null, Array.Empty<object>());
        await task;
    }

    private static async Task<byte[]> GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        using var rs = self.GetManifestResourceStream(name);

        if (rs is null)
            return Array.Empty<byte>();

        using var ms = new MemoryStream();
        await rs.CopyToAsync(ms);

        return ms.ToArray();
    }
}