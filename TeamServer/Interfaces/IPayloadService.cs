using TeamServer.Handlers;

namespace TeamServer.Interfaces;

public interface IPayloadService
{
    Task<byte[]> GeneratePayload(Handler handler, PayloadFormat format);
}

public enum PayloadFormat
{
    EXE,
    DLL,
    SVC_EXE,
    POWERSHELL,
    SHELLCODE
}