using Newtonsoft.Json;
using System;
using System.Configuration;

namespace StreamExchangeRate_v._2.Binance
{
    class BinanceClient : AProvider
    {
        public BinanceClient(string key)
        {
            Key = key;
            NameProvider = "Binance";
        }

        public override Uri getUri()
        {
            Uri uri = null;
            try
            {
                string streamName = string.Empty;
                // array of symbols from the file App.config
                Symbol = ConfigurationManager.AppSettings.Get(Key).Split((string[])null, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in Symbol)
                {
                    streamName += $"{s.ToLower()}@ticker/";
                }
                uri = new Uri($"wss://stream.binance.com:9443/stream?streams={streamName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error for key {Key} \nex.Message: {ex.Message}");
            }
            return uri;
        }

        public override void OnMessage(string data)
        {
            // get socket data
            BinanceStreamTick eventData = JsonConvert.DeserializeObject<BinanceStreamTick>(data);
            BaseTicker ticker = new BaseTicker();
            ticker.Symbol = eventData.Data.Symbol;
            ticker.AskPrice = eventData.Data.BestAskPrice;
            ticker.BidPrice = eventData.Data.BestBidPrice;
            ticker.TotalTradedVolume = eventData.Data.TotalTradedQuoteAssetVolume;

            // check the data has changed
            if (!EqualsTicker(ticker))
                Console.WriteLine($"{ticker.Symbol} : Ask = {ticker.AskPrice}  Bid = {ticker.BidPrice} Volume = {ticker.TotalTradedVolume}");
            else
                Console.WriteLine($"{ticker.Symbol}: data not changed");
        }
    }
}

