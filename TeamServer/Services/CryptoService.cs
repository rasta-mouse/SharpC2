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
    
    public async Task<(byte[] iv, byte[] data, byte[] checksum)> EncryptObject<T>(T obj)
    {
        var key = await GetKey();
        
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = key;
        aes.GenerateIV();

        using var transform = aes.CreateEncryptor();
        
        var raw = obj.Serialize();
        var enc = transform.TransformFinalBlock(raw, 0, raw.Length);
        var checksum = ComputeHmac(enc, key);

        return (aes.IV, enc, checksum);
    }

    public async Task<T> DecryptObject<T>(byte[] iv, byte[] data, byte[] checksum)
    {
        var key = await GetKey();
        
        if (!ComputeHmac(data, key).SequenceEqual(checksum))
            throw new CryptoException("Invalid Checksum");

        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = key;
        aes.IV = iv;

        using var transform = aes.CreateDecryptor();
        var dec = transform.TransformFinalBlock(data, 0, data.Length);

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

public class CryptoException : Exception
{
    public CryptoException() { }
    public CryptoException(string message) : base(message) { }
    public CryptoException(string message, Exception inner) : base(message, inner) { }
}