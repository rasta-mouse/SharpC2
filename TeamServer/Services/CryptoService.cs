using System.Security.Cryptography;

using TeamServer.Interfaces;
using TeamServer.Storage;
using TeamServer.Utilities;

namespace TeamServer.Services;

public class CryptoService : ICryptoService
{
    private readonly IDatabaseService _db;

    public CryptoService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task<byte[]> Encrypt<T>(T item)
    {
        var key = await GetKey();
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = key;
        aes.GenerateIV();

        using var transform = aes.CreateEncryptor();
        
        var raw = item.Serialize();
        var enc = transform.TransformFinalBlock(raw, 0, raw.Length);
        var checksum = ComputeHmac(enc, key);

        var buf = new byte[aes.IV.Length + checksum.Length + enc.Length];
        
        Buffer.BlockCopy(aes.IV, 0, buf, 0, aes.IV.Length);
        Buffer.BlockCopy(checksum, 0, buf, aes.IV.Length, checksum.Length);
        Buffer.BlockCopy(enc, 0, buf, aes.IV.Length + checksum.Length, enc.Length);

        return buf;
    }

    public async Task<T> Decrypt<T>(byte[] data)
    {
        // iv 16 bytes
        // hmac 32 bytes
        // data n bytes
        
        var iv = data[..16];
        var checksum = data[16..(16 + 32)];
        var enc = data[(16 + 32)..];

        var key = await GetKey();
        
        if (!ComputeHmac(enc, key).SequenceEqual(checksum))
            throw new Exception("Invalid Checksum");

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = key;
        aes.IV = iv;

        using var transform = aes.CreateDecryptor();
        var dec = transform.TransformFinalBlock(enc, 0, enc.Length);

        return dec.Deserialize<T>();
    }

    public async Task<byte[]> GetKey()
    {
#if DEBUG
        return Convert.FromBase64String("TfiAGr88Ia1PHiFHxTVMTf5/qXzhgN2nnn4TvsXYUQo=");
#endif
        
        var conn = _db.GetAsyncConnection();
        var dao = await conn.Table<CryptoDao>().FirstOrDefaultAsync();

        // if key was null, create one
        if (dao is null)
        {
            dao = new CryptoDao { Key = GenerateRandomKey() };
            await conn.InsertAsync(dao);
        }

        return dao.Key;
    }
    
    private static byte[] ComputeHmac(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }
    
    private static byte[] GenerateRandomKey()
    {
        var buf = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(buf);

        return buf;
    }
}