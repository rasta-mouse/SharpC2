using System;
using System.IO;
using System.Reflection;

namespace Drone;

public static class Helpers
{
    public static byte[] GetEmbeddedResource(string name)
    {
        var self = Assembly.GetExecutingAssembly();
        using var rs = self.GetManifestResourceStream(name);

        if (rs is null)
            return Array.Empty<byte>();

        using var ms = new MemoryStream();
        rs.CopyTo(ms);

        return ms.ToArray();
    }
}