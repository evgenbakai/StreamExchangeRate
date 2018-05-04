using System;
using System.Configuration;
using Newtonsoft.Json;
using WebSocketSharp;
using System.Threading;
namespace StreamExchangeRate.Binance
{
    class BinanceClient : Provider
    {
        public BinanceClient(string key) : base(key, "wss://stream.binance.com:9443")
        {
            try
            {
                string streamName = "";
                // array of symbols from the file App.config
                symbol = ConfigurationManager.AppSettings.Get(key).Split((string[])null, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in symbol)
                {
                    streamName += $"{s.ToLower()}@ticker/";
                }
                serverUri = new Uri($"{baseWebsocketUri}/stream?streams={streamName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error for key {key} \nex.Message: {ex.Message}");
            }
        }
        
        protected override string NameProvider { get; set; } = "Binance";

        protected override void OnMessage(string data)
        {
            // get socket data
            BinanceStreamTick eventData = JsonConvert.DeserializeObject<BinanceStreamTick>(data);
            BaseTicker ticker = new BaseTicker()
            {
                Symbol = eventData.Data.Symbol,
                AskPrice = eventData.Data.BestAskPrice,
                BidPrice = eventData.Data.BestBidPrice,
                TotalTradedVolume = eventData.Data.TotalTradedQuoteAssetVolume
            };
            // check the data has changed
            if (!EqualsTicker(ticker))
                Console.WriteLine($"{ticker.Symbol} : Ask = {ticker.AskPrice}  Bid = {ticker.BidPrice} Volume = {ticker.TotalTradedVolume}");
            else
                Console.WriteLine($"{ticker.Symbol}: data not changed");
        }
    }
}
