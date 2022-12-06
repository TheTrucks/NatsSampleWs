using Microsoft.Extensions.Options;
using NATS_WS.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace NATS_WS.Services
{
    public class WSSenderOptions
    {
        public int SecondsBack { get; set; }
    }
    public class WSSender : IDisposable
    {
        private readonly NatsReader _reader;
        private readonly WSSenderOptions _opts;
        private ConcurrentDictionary<string, WebSocket> ClientsList = new();
        private readonly PeriodicTimer _timer;
        private readonly Task PeriodicTask;
        private bool disposed = false;

        public WSSender(NatsReader reader, IOptions<WSSenderOptions> opts)
        {
            _reader = reader;
            _opts = opts.Value;
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(_opts.SecondsBack));
            PeriodicTask = Task.Factory.StartNew(() => ReadSendLoop());
            PeriodicTask.ConfigureAwait(false);
        }

        private async Task ReadSendLoop()
        {
            while (!disposed)
            {
                await _timer.WaitForNextTickAsync();
                while (ClientsList.Count == 0)
                    await _timer.WaitForNextTickAsync();
                var data = _reader.ReadStream();
                Console.WriteLine($"Received {data.Count}");
                foreach (var item in data) 
                {
                    foreach (var client in ClientsList) 
                    {
                        await client.Value.SendAsync(item.Data, WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    item.Ack();
                }
            }
        }

        public async Task Subscribe(WebSocket Client, string ClientId)
        {
            if (ClientsList.ContainsKey(ClientId)) 
            {
                await Unsubscribe(ClientId);
            }
            else
                ClientsList.TryAdd(ClientId, Client);

            Console.WriteLine($"Subbed {ClientId}");
        }

        public async Task Unsubscribe(string ClientId)
        {
            if (ClientsList.ContainsKey(ClientId))
            {
                if (ClientsList.TryGetValue(ClientId, out var ExistingClient))
                {
                    await ExistingClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "unsubbing", CancellationToken.None);
                    ExistingClient.Dispose();
                    ClientsList.TryRemove(ClientId, out _);

                    Console.WriteLine($"Unsubbed {ClientId}");
                }
            }
        }

        public void Dispose()
        {
            if (!disposed) 
            {
                PeriodicTask.Dispose();
                _timer.Dispose();
                foreach (var client in ClientsList) 
                {
                    client.Value.Dispose();
                }
                disposed = true;
            }
        }
    }
}
