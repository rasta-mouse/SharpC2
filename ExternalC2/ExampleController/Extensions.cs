using System.IO.Pipes;
using System.Net.Sockets;

namespace ExampleController;

public static class Extensions
{
    public static bool DataAvailable(this PipeStream pipe)
    {
        var hPipe = pipe.SafePipeHandle.DangerousGetHandle();
        
        uint available = 0;
        
        _ = Win32.PeekNamedPipe(
            hPipe,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            ref available,
            IntPtr.Zero);

        return available > 0;
    }

    public static bool DataAvailable(this NetworkStream stream)
    {
        return stream.DataAvailable;
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
}