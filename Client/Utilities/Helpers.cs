namespace SharpC2.Utilities;

public static class Helpers
{
    public static async Task<byte[]> GetDefaultResource(string name, bool x64)
    {
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Resources",
            x64 ? "x64" : "x86",
            name);
        
        return await File.ReadAllBytesAsync(path);
    }
}