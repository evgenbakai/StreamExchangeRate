using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamExchangeRate
{
    abstract class Provider
    {
        protected string baseWebsocketUri;         // wss://stream.binance.com:9443
        protected Uri serverUri;                   // wss://stream.binance.com:9443/stream?streams=symbol@ticker/symbol@ticker
        protected List<BaseTicker> listTicker;     // list with data for everyone symbol
        protected string[] symbol;                 // array symbols
        // ------------------------------------------------------------
        protected ClientWebSocket webSocket;
        protected CancellationTokenSource cancellationTokenSource;
        protected const int ReceiveChunkSize = 1024;
        public bool IsConnected;
        Timer timerIsPing;

        abstract protected void OnMessage(string data);

        abstract protected string NameProvider { get; set; }

        public Provider(string key, string baseWebsocketUri)
        {
            this.baseWebsocketUri = baseWebsocketUri;
            this.listTicker = new List<BaseTicker>();
        }

        public async Task /*void*/ ConnectAsync()
        {
            if (webSocket != null)
            {
                Console.WriteLine($"\n{NameProvider} Reconnect...");
                Disconnect();
            }
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();
            webSocket.Options.KeepAliveInterval = new TimeSpan(0, 0, 0, 10);
            try
            {
                await webSocket.ConnectAsync(serverUri, cancellationTokenSource.Token);
                if (webSocket.State == WebSocketState.Open)
                {
                    IsConnected = true;
                    _pinger();
                    Console.WriteLine($"{NameProvider} WebSocket Open");
                    Console.WriteLine($"{NameProvider} subscribed to {symbol.Length} pairs: {string.Join(" ", symbol).ToUpper()} \n");
                    StartListen();
                }
                else
                {
                    Console.WriteLine($"{NameProvider} Error in opening WebSocket");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{NameProvider} Error in opening WebSocket: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                }
                if (webSocket != null)
                {
                    webSocket.Abort();
                    webSocket.Dispose();
                    Console.WriteLine($"\n{NameProvider} WebSocket Close.");
                }
                if (timerIsPing != null)
                {
                    timerIsPing.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnect error: " + ex);
            }
            finally
            {
                webSocket = null;
                cancellationTokenSource = null;
                timerIsPing = null;
                IsConnected = false;
            }
        }

        private async void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    string strResult = string.Empty;
                    WebSocketReceiveResult result = null;
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            strResult = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationTokenSource.Token);
                        }
                    } while (!result.EndOfMessage);
                    OnMessage(strResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine($"{NameProvider} Error in ReceiveAsync: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                Console.WriteLine($"\n{NameProvider} WebSocket Stop.");
            }
        }

        private void _pinger()
        {
            TimerCallback timerCallback = new TimerCallback(delegate
            {
                if (webSocket != null && webSocket.State != WebSocketState.Open && IsConnected)
                {
                    IsConnected = false;
                }
                else if (webSocket == null && IsConnected)
                {
                    IsConnected = false;
                }
                //Console.WriteLine($"\nIsConnected = {IsConnected},  webSocket.State = {webSocket.State}\n");
            });
            if (timerIsPing != null)
                timerIsPing.Dispose();

            timerIsPing = new Timer(timerCallback);
            timerIsPing.Change(0, 2000);
        }

        protected bool EqualsTicker(BaseTicker ticker)
        {
            // return true -  в списку listTicker знайдено обєкт який рівний обєкту  ticker
            // return false - в списку listTicker не знайдено обєкта по необхідному символу (ticker.Symbol), або
            //                відповідний обєкт з listTicker не рівнмй обєкту ticker
            bool returnValue;
            int indexFoundTicker = listTicker.FindIndex((t) => t.Symbol == ticker.Symbol);
            if (indexFoundTicker != -1)
            {
                returnValue = listTicker[indexFoundTicker].Equals(ticker);
                listTicker[indexFoundTicker] = ticker;
                return returnValue;   
            }
            else
            {
                listTicker.Add(ticker);
                return false;
            }
        }
    }
}