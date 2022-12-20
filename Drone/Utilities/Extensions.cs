using System;
using System.IO;
using System.Security;

using ProtoBuf;

namespace Drone.Utilities;

public static class Extensions
{
    public static byte[] Serialize<T>(this T obj)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, obj);
        return ms.ToArray();
    }

    public static T Deserialize<T>(this byte[] data)
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }

    public static SecureString ToSecureString(this string value)
    {
        var secure = new SecureString();

        foreach (var c in value)
            secure.AppendChar(c);

        return secure;
    }
    
    public static void Clear(this MemoryStream stream)
    {
        var buffer = stream.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        stream.Position = 0;
        stream.SetLength(0);
    }
}