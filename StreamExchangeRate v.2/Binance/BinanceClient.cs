using Newtonsoft.Json;
using System;

namespace StreamExchangeRate_v._2.Binance
{
    class BinanceClient : AProvider
    {
        public BinanceClient(string providerId) : base()
        {
            ProviderId = providerId;
            ProviderName = "Binance";
        }

        public override Uri GetUri()
        {
            Uri uri = null;
            try
            {
                string streamName = string.Empty;
                Symbol = _mappings[ProviderId].getListSymbol();
                _mappings[ProviderId].getListSymbolApi().ForEach(s => streamName += $"{s}@ticker/");
                uri = new Uri($"wss://stream.binance.com:9443/stream?streams={streamName.ToLower()}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error for ProviderId: {ProviderId} \nex.Message: {ex.Message}");
            }
            return uri;
        }

        public override void OnMessage(string data)
        {
            // get socket data
            BinanceStreamTick eventData = JsonConvert.DeserializeObject<BinanceStreamTick>(data);
            BaseTicker ticker = new BaseTicker();
            ticker.Symbol = _mappings[ProviderId].getSymbol(eventData.Data.SymbolApi);
            ticker.AskPrice = eventData.Data.BestAskPrice;
            ticker.BidPrice = eventData.Data.BestBidPrice;
            ticker.TotalTradedVolume = eventData.Data.TotalTradedQuoteAssetVolume;

            // check the data has changed
            if (processTicker(ticker))
                Console.WriteLine($"[{ProviderName}] {ticker.Symbol} : Ask = {ticker.AskPrice}  Bid = {ticker.BidPrice} Volume = {ticker.TotalTradedVolume}");
            else
                Console.WriteLine($"[{ProviderName}] {ticker.Symbol}: data not changed");
        }
    }
}

