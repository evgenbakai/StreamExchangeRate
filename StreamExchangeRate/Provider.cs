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
        protected CancellationToken cancellationToken;
        protected const int ReceiveChunkSize = 1024;
        protected bool flagStartStop;

        abstract protected void OnMessage(string data);

        abstract protected string NameProvider { get; set; }

        public Provider(string key, string baseWebsocketUri)
        {
            this.baseWebsocketUri = baseWebsocketUri;
            this.listTicker = new List<BaseTicker>();
        }

        ~Provider()
        {
            Dispose();
        }

        public async Task ConnectAsync()
        {
            this.webSocket = new ClientWebSocket();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = cancellationTokenSource.Token;
            try
            {
                await webSocket.ConnectAsync(serverUri, cancellationToken);
                if (webSocket.State == WebSocketState.Open)
                {
                    Console.WriteLine($"{NameProvider} WebSocket Open");
                    Console.WriteLine($"{NameProvider} subscribed to {symbol.Length} pairs: {string.Join(" ", symbol).ToUpper()} \n");
                    flagStartStop = true;
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

        public void Dispose()
        {
            if (webSocket != null)
            {
                cancellationTokenSource.Cancel();
                webSocket.Dispose();
                webSocket = null;
            }
        }

        public async Task Disconnect()
        {
            try
            {
                if (webSocket != null)
                {
                    if (webSocket.State != WebSocketState.Closed)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "DELETE", CancellationToken.None);
                    }
                    webSocket.Dispose();
                    Console.WriteLine($"\n{NameProvider} WebSocket Close.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnect error: " + ex);
            }
            finally
            {
                webSocket = null;
            }
        }


        private async void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];
            try
            {
                while (webSocket.State == WebSocketState.Open && flagStartStop)
                {
                    string strResult = string.Empty;
                    WebSocketReceiveResult result = null;
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            strResult = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        }
                    } while (!result.EndOfMessage);
                    OnMessage(strResult);
                }
            }
            catch (Exception)
            {
                //Reconnect();
            }
        }

        private void Reconnect()
        {
            Timer timer = null;
            TimerCallback timerCallback = new TimerCallback(async delegate
            {
                if(webSocket.State != WebSocketState.Open)
                {
                    Console.WriteLine($"{NameProvider}: Reconnect...");
                    webSocket.Dispose();
                    await ConnectAsync();
                }
                else
                {
                    //timer.Dispose();
                }

            });
            timer = new Timer(timerCallback, null, 0, 1000);
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