namespace SharpC2.Models;

public sealed class Metadata
{
    public string Id { get; set; }
    public string Address { get; set; }
    public string Identity { get; set; }
    public string Hostname { get; set; }
    public string Process { get; set; }
    public int ProcessId { get; set; }
    public ProcessIntegrity Integrity { get; set; }
    public ProcessArch Architecture { get; set; }
}

public enum ProcessIntegrity
{
    MEDIUM,
    HIGH,
    SYSTEM
}

public enum ProcessArch
{
    X86,
    X64
}