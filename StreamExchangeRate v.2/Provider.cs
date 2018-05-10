using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamExchangeRate_v._2
{
    public class Provider : IDisposable
    {
        private readonly Uri _url;
        private readonly Timer _lastChanceTimer;
        private AProvider provider;

        private DateTime _lastReceivedMsg = DateTime.UtcNow;
        private bool _disposing = false;
        private ClientWebSocket _client;
        private CancellationTokenSource _cancelation;

        public Provider(AProvider provider)
        {
            if (provider == null)
            {
                throw new NullReferenceException();
            }

            this.provider = provider;
            _url = provider.GetUri();
            int minute = 1000 * 60;
            _lastChanceTimer = new Timer(async x => await LastChance(x), null, minute, minute);
        }

        public void Dispose()
        {
            _disposing = true;
            Console.WriteLine(L("Disposing.."));

            _lastChanceTimer?.Dispose();
            _cancelation?.Cancel();
            _client?.Abort();
            _client?.Dispose();
            _cancelation?.Dispose();
        }

        public Task Start()
        {
            Console.WriteLine(L("Starting.."));
            _cancelation = new CancellationTokenSource();
            return StartClient(_url, _cancelation.Token);
        }

        private async Task StartClient(Uri uri, CancellationToken token)
        {
            _client = new ClientWebSocket()
            {
                Options = { KeepAliveInterval = new TimeSpan(0, 0, 0, 10) }
            };

            try
            {
                await _client.ConnectAsync(uri, token);
                if (_client.State == WebSocketState.Open)
                    Console.WriteLine($"{provider.ProviderName} subscribed to {provider.Symbol.Count} pairs: {string.Join(" ", provider.Symbol).ToUpper()} \n");
#pragma warning disable 4014
                /*await*/ Listen(_client, token);
#pragma warning restore 4014

            }
            catch (Exception e)
            {
                Console.WriteLine(L($"Exception while connecting: {e.Message}"));
                await Reconnect();
            }
        }

        private async Task Reconnect()
        {
            if (_disposing)
                return;

            Console.WriteLine(L("Reconnecting..."));
            _cancelation.Cancel();
            await Task.Delay(10000);

            _cancelation = new CancellationTokenSource();
            await StartClient(_url, _cancelation.Token);
        }

        private async Task Listen(ClientWebSocket client, CancellationToken token)
        {
            do
            {
                WebSocketReceiveResult result = null;
                var buffer = new byte[1024];
                var message = new ArraySegment<byte>(buffer);
                var resultMessage = new StringBuilder();
                do
                {
                    result = await client.ReceiveAsync(message, token);
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    resultMessage.Append(receivedMessage);
                    if (result.MessageType != WebSocketMessageType.Text)
                        break;

                } while (!result.EndOfMessage);

                var received = resultMessage.ToString();
                _lastReceivedMsg = DateTime.UtcNow;
                provider.OnMessage(received);
            } while (client.State == WebSocketState.Open && !token.IsCancellationRequested);
        }

        private async Task LastChance(object state)
        {
            var diffMin = Math.Abs(DateTime.UtcNow.Subtract(_lastReceivedMsg).TotalMinutes);
            if (diffMin > 1)
                Console.WriteLine(L($"Last message received {diffMin} min ago"));
            if (diffMin > 3)
            {
                Console.WriteLine(L("Last message received more than 3 min ago. Hard restart.."));

                _client?.Abort();
                _client?.Dispose();
                await Reconnect();
            }
        }

        private string L(string msg)
        {
            return $"[{provider.ProviderName} WEBSOCKET] {msg}";
        }
    }
}