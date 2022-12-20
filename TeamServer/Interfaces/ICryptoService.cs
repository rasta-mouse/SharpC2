namespace TeamServer.Interfaces;

public interface ICryptoService
{
    Task<byte[]> Encrypt<T>(T item);
    Task<T> Decrypt<T>(byte[] data);
    Task<byte[]> GetKey();
}