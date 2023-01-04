using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Security;
using System.Threading.Tasks;
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
    
    public static bool DataAvailable(this TcpClient client)
    {
        var stream = client.GetStream();
        return stream.DataAvailable;
    }

    public static bool DataAvailable(this PipeStream pipe)
    {
        var hPipe = pipe.SafePipeHandle.DangerousGetHandle();
        return Interop.Methods.PeekNamedPipe(hPipe);
    }

    public static async Task<byte[]> ReadStream(this Stream stream)
    {
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
    
    public static async Task WriteStream(this Stream stream, byte[] data)
    {
        // format data as [length][value]
        var lengthBuf = BitConverter.GetBytes(data.Length);
        var lv = new byte[lengthBuf.Length + data.Length];

        Buffer.BlockCopy(lengthBuf, 0, lv, 0, lengthBuf.Length);
        Buffer.BlockCopy(data, 0, lv, lengthBuf.Length, data.Length);
        
        using var ms = new MemoryStream(lv);
        
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

    public static async Task<byte[]> ReadClient(this TcpClient client)
    {
        var stream = client.GetStream();
        
        using var ms = new MemoryStream();
        int read;
        
        do
        {
            var buf = new byte[1024];
            read = await stream.ReadAsync(buf, 0, buf.Length);
            
            if (read == 0)
                break;

            await ms.WriteAsync(buf, 0, read);
        }
        while (read >= 1024);
        
        return ms.ToArray();
    }

    public static async Task WriteClient(this TcpClient client, byte[] data)
    {
        var stream = client.GetStream();
        await stream.WriteAsync(data, 0, data.Length);
    }
}