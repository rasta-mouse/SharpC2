using System.Security;
using System.Security.Claims;
using ProtoBuf;

namespace TeamServer.Utilities;

public static class Extensions
{
    public static byte[] Serialize<T>(this T item)
    {
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, item);
        return ms.ToArray();
    }

    public static T Deserialize<T>(this byte[] data)
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }

    public static string GetClaimFromContext(this HttpContext context)
    {
        return context.User.Identity is not ClaimsIdentity identity
            ? string.Empty
            : identity.Name;
    }

    public static async Task<byte[]> ReadStream(this Stream stream)
    {
        const int bufSize = 1024;
        int read;

        using var ms = new MemoryStream();

        do
        {
            var buf = new byte[bufSize];
            read = await stream.ReadAsync(buf, 0, bufSize);

            if (read == 0)
                break;

            await ms.WriteAsync(buf, 0, read);

        } while (read >= bufSize);

        return ms.ToArray();
    }
}