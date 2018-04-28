using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace StreamExchangeRate
{
    abstract class Provider
    {
        protected string baseWebsocketUri;         // wss://stream.binance.com:9443
        protected Uri serverUri;                   // wss://stream.binance.com:9443/stream?streams=symbol@ticker/symbol@ticker
        protected List<MyTicker> listMyTicker;     // list with data for everyone symbol
        protected string[] symbol;                 // array symbols
        protected WebSocket socket;                // object WebSocket
        private int retry;                         // counter 

        abstract protected void OnMessage(object sender, MessageEventArgs e);
        abstract protected string NameProvider { get; set; }

        public Provider(string key, string baseWebsocketUri)
        {
            this.baseWebsocketUri = baseWebsocketUri;
            listMyTicker = new List<MyTicker>();
        }
        ~Provider()
        {
            if (socket != null)
            {
                socket.CloseAsync();
                socket = null;
            }
        }

        public void ConnectAsync()
        {
            socket = new WebSocket(serverUri.AbsoluteUri);
            socket.OnOpen += OnOpen;
            socket.OnMessage += OnMessage;
            socket.OnError += OnError;
            socket.OnClose += OnClose;
            socket.ConnectAsync();
        }

        public void DisconnectAsync()
        {
            if (socket != null)
            {
                socket.CloseAsync();
                socket = null;
            }
        }

        protected void OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine($"{NameProvider} WebSocket Open");
            Console.WriteLine($"{NameProvider} subscribed to {symbol.Length} pairs: {string.Join(" ", symbol).ToUpper()} \n");
        }

        protected void OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine($"\n{NameProvider} WebSocket Close. " + e.Reason + " --- " + e.Code);

            if (e.WasClean && e.Code != (ushort)CloseStatusCode.Abnormal)
                return;

            if (retry < 5)
            {
                Console.WriteLine($"{NameProvider}: Reconnect...");
                retry++;
                Thread.Sleep(5000);
                socket.ConnectAsync();
            }
            else
            {
                Console.WriteLine($"{NameProvider}: The reconnecting has failed.");
            }
        }

        protected void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"{NameProvider} WebSocket Error Message:" + e.Message);
        }

        protected bool MyEqualsTicker(MyTicker ticker)
        {
            bool flag = false;
            for (int i = 0; i < listMyTicker.Count; i++)
            {
                if (listMyTicker[i].Symbol == ticker.Symbol)
                {
                    flag = true;
                    if (listMyTicker[i].Equals(ticker))
                    {
                        return false;
                    }
                }
            }
            if (!flag)
                listMyTicker.Add(ticker);
            return true;
        }
        
        //private Task IsAlive()
        //{
        //    while (true)
        //    {
        //        if (socket != null && socket.IsAlive)
        //        {
        //            Console.WriteLine("connected");
        //        }
        //        else
        //        {
        //            Console.WriteLine("not connected");
        //        }
        //    }
        //}
        //public async void Alive()
        //{
        //    await IsAlive();
        //}
    }
}