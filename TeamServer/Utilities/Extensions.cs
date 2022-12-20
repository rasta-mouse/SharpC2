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
}