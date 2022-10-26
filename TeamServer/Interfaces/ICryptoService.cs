namespace TeamServer.Interfaces;

public interface ICryptoService
{
    Task<(byte[] iv, byte[] data, byte[] checksum)> EncryptObject<T>(T obj);
    Task<T> DecryptObject<T>(byte[] iv, byte[] data, byte[] checksum);
    Task<byte[]> GetKey();
}