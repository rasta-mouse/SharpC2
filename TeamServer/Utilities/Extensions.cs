using System.Collections;
using System.Net.Sockets;
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
    
    public static bool DataAvailable(this TcpClient client)
    {
        var stream = client.GetStream();
        return stream.DataAvailable;
    }
    
    public static async Task<byte[]> ReadClient(this TcpClient client)
    {
        var stream = client.GetStream();
        
        // read length
        var lengthBuf = new byte[4];
        var read = await stream.ReadAsync(lengthBuf, 0, 4);

        if (read != 4)
            throw new Exception("Failed to read length");

        var length = BitConverter.ToInt32(lengthBuf, 0);
        
        // read rest of data
        using var ms = new MemoryStream();
        var totalRead = 0;
        
        do
        {
            var buf = new byte[1024];
            read = await stream.ReadAsync(buf, 0, buf.Length);

            await ms.WriteAsync(buf, 0, read);
            totalRead += read;
        }
        while (totalRead < length);
        
        return ms.ToArray();
    }
    
    public static async Task WriteClient(this TcpClient client, byte[] data)
    {
        // format data as [length][value]
        var lengthBuf = BitConverter.GetBytes(data.Length);
        var lv = new byte[lengthBuf.Length + data.Length];

        Buffer.BlockCopy(lengthBuf, 0, lv, 0, lengthBuf.Length);
        Buffer.BlockCopy(data, 0, lv, lengthBuf.Length, data.Length);
        
        using var ms = new MemoryStream(lv);
        var stream = client.GetStream();
        
        // write in chunks
        var bytesRemaining = lv.Length;
        do
        {
            var lengthToSend = bytesRemaining < 1024 ? bytesRemaining : 1024;
            var buf = new byte[lengthToSend];
            
            var read = await ms.ReadAsync(buf, 0, lengthToSend);

            if (read != lengthToSend)
                throw new Exception("Could not read data from stream");
            
            await stream.WriteAsync(buf, 0, buf.Length);
            
            bytesRemaining -= lengthToSend;
        }
        while (bytesRemaining > 0);
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