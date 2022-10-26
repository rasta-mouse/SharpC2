namespace Drone.Interfaces;

public interface ICrypto
{
    (byte[] iv, byte[] data, byte[] checksum) EncryptObject<T>(T obj);
    T DecryptObject<T>(byte[] iv, byte[] data, byte[] checksum);
}