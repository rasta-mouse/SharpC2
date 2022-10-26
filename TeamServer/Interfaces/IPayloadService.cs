using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IPayloadService
{
    Task<byte[]> GeneratePayload(Handler handler, PayloadFormat format);
    Task<byte[]> GenerateHttpPayload(string connectAddress, int connectPort, bool secure, PayloadFormat format);
    Task<byte[]> GenerateBindTcpPayload(int bindPort, bool loopbackOnly, PayloadFormat format);
    Task<byte[]> GenerateBindSmbPayload(string pipeName, PayloadFormat format);
}

public enum PayloadFormat
{
    Exe,
    Dll,
    ServiceExe,
    PowerShell,
    Shellcode
}