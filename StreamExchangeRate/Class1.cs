using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace StreamExchangeRate
{
    public class BitMexClient
    {

        private ClientWebSocket _ws;

        private string _serverAdress;

        /// <summary>
        /// адрес сервера для подключения через websocket
        /// </summary>
        public string ServerAdres
        {
            set { _serverAdress = value; }
            private get { return _serverAdress; }
        }


        public bool IsConnected;

        /// <summary>
        /// установить соединение с сервером
        /// </summary>
        public void Connect()
        {
            if (_ws != null)
            {
                Disconnect();
            }

            _ws = new ClientWebSocket();

            Uri uri = new Uri(_serverAdress);
            _ws.ConnectAsync(uri, CancellationToken.None).Wait();

            if (_ws.State == WebSocketState.Open)
            {
                if (Connected != null)
                {
                    Connected.Invoke();
                }
                IsConnected = true;
            }

            Thread worker = new Thread(GetRes);
            worker.CurrentCulture = new CultureInfo("ru-RU");
            worker.IsBackground = true;
            worker.Start(_ws);

            Thread converter = new Thread(Converter);
            converter.CurrentCulture = new CultureInfo("ru-RU");
            converter.IsBackground = true;
            converter.Start();

            Thread wspinger = new Thread(_pinger);
            wspinger.CurrentCulture = new CultureInfo("ru-RU");
            wspinger.IsBackground = true;
            wspinger.Start();


        }

        /// <summary>
        /// отключение
        /// </summary>
        public void Disconnect()
        {
            if (_ws != null)
            {
                _ws.Abort();
                _ws.Dispose();
                Thread.Sleep(1000);
                _ws = null;
            }
            IsConnected = false;
            if (Disconnected != null)
            {
                Disconnected();
            }
        }

        /// <summary>
        /// проверка соединения
        /// </summary>
        private void _pinger()
        {
            while (true)
            {
                Thread.Sleep(10000);

                if (_ws != null && _ws.State != WebSocketState.Open && IsConnected)
                {
                    IsConnected = false;

                    if (Disconnected != null)
                    {
                        Disconnected();
                    }
                }
            }
        }



        /// <summary>
        /// очередь новых сообщений, пришедших с сервера биржи
        /// </summary>
        private ConcurrentQueue<string> _newMessage = new ConcurrentQueue<string>();

        /// <summary>
        /// метод, который в отдельном потоке принимает все новые сообщения от биржи и кладет их в общую очередь
        /// </summary>
        /// <param name="clientWebSocket">вебсокет клиент</param>
        private void GetRes(object clientWebSocket)
        {
            ClientWebSocket ws = (ClientWebSocket)clientWebSocket;

            string res = "";

            while (true)
            {
                try
                {
                    if (IsConnected)
                    {
                        var buffer = new ArraySegment<byte>(new byte[1024]);
                        var result = ws.ReceiveAsync(buffer, CancellationToken.None).Result;

                        if (result.Count == 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        if (result.EndOfMessage == false)
                        {
                            res += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        }
                        else
                        {
                            res += Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            _newMessage.Enqueue(res);
                            res = "";
                        }
                    }
                }
                catch (Exception exception)
                {
                    //if (BitMexLogMessageEvent != null)
                    //{
                    //    BitMexLogMessageEvent(exception, LogMessageType.System);
                    //}
                }

            }
        }

        /// <summary>
        /// метод конвертирует JSON в классы C# и отправляет на верх
        /// </summary>
        private void Converter()
        {
            while (true)
            {
                try
                {
                    if (!_newMessage.IsEmpty)
                    {
                        string mes;

                        if (_newMessage.TryDequeue(out mes))
                        {
                            if (mes.Contains("error"))
                            {
                                if (ErrorEvent != null)
                                {
                                    ErrorEvent(mes);
                                }
                            }

                        }
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }


        /// <summary>
        /// ошибка http запроса или websocket
        /// </summary>
        public event Action<string> ErrorEvent;

        /// <summary>
        /// соединение с BitMEX API установлено
        /// </summary>
        public event Action Connected;

        /// <summary>
        /// соединение с BitMEX API разорвано
        /// </summary>
        public event Action Disconnected;



    }
}
