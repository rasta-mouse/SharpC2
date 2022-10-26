using ProtoBuf;

namespace SharpC2.Utilities;

public static class Extensions
{
    public static T Deserialize<T>(this byte[] data)
    {
        using var ms = new MemoryStream(data);
        return Serializer.Deserialize<T>(ms);
    }
}