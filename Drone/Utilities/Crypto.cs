using System;
using System.Linq;
using System.Security.Cryptography;

namespace Drone.Utilities;

public static class Crypto
{
    public static byte[] Encrypt<T>(T item)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = Key;
        aes.GenerateIV();

        using var transform = aes.CreateEncryptor();
        
        var raw = item.Serialize();
        var enc = transform.TransformFinalBlock(raw, 0, raw.Length);
        var checksum = ComputeHmac(enc);
        
        var buf = new byte[aes.IV.Length + checksum.Length + enc.Length];
        
        Buffer.BlockCopy(aes.IV, 0, buf, 0, aes.IV.Length);
        Buffer.BlockCopy(checksum, 0, buf, aes.IV.Length, checksum.Length);
        Buffer.BlockCopy(enc, 0, buf, aes.IV.Length + checksum.Length, enc.Length);

        return buf;
    }

    public static T Decrypt<T>(byte[] data)
    {
        var iv = new byte[16];
        Buffer.BlockCopy(data, 0, iv, 0, iv.Length);

        var checksum = new byte[32];
        Buffer.BlockCopy(data, 16, checksum, 0, checksum.Length);

        var enc = new byte[data.Length - 48];
        Buffer.BlockCopy(data, 48, enc, 0, data.Length - 48);

        if (!ComputeHmac(enc).SequenceEqual(checksum))
            throw new Exception("Invalid Checksum");
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = Key;
        aes.IV = iv;

        using var transform = aes.CreateDecryptor();
        var dec = transform.TransformFinalBlock(enc, 0, enc.Length);

        return dec.Deserialize<T>();
    }

    private static byte[] ComputeHmac(byte[] data)
    {
        using var hmac = new HMACSHA256(Key);
        return hmac.ComputeHash(data);
    }

    private static byte[] Key
        => Convert.FromBase64String("TfiAGr88Ia1PHiFHxTVMTf5/qXzhgN2nnn4TvsXYUQo=");
}