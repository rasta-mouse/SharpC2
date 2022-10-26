using dnlib.DotNet;

using ProtoBuf;

namespace TeamServer.Utilities;

public static class Extensions
{
    public static string ToShortGuid(this Guid guid)
    {
        return Guid.NewGuid().ToString().Replace("-", "")[..10];
    }
    
    public static (byte[] iv, byte[] data, byte[] checksum) FromByteArray(this byte[] bytes)
    {
        // static sizes
        // iv 16 bytes
        // hmac 32 bytes
        // data n bytes
        
        var iv = bytes[..16];
        var checksum = bytes[16..(16 + 32)];
        var data = bytes[(16 + 32)..];

        return (iv, data, checksum);
    }
    
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

    public static TypeDef GetTypeDef(this ModuleDef module, string name)
    {
        return module.Types.Single(t => t.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public static MethodDef GetMethodDef(this TypeDef type, string name)
    {
        return type.Methods.Single(m => m.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
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
        var l = BitConverter.GetBytes(data.Length);
        var lv = new byte[l.Length + data.Length];

        Buffer.BlockCopy(l, 0, lv, 0, l.Length);
        Buffer.BlockCopy(data, 0, lv, l.Length, data.Length);
        
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