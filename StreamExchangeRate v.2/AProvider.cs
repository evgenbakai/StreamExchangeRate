using System;
using System.Collections.Concurrent;
using StreamExchangeRate_v._2.Config;
using System.Collections.Generic;
using System.Linq;

namespace StreamExchangeRate_v._2
{
    public abstract class AProvider
    {
        TotalTicker totalTicker;

        public AProvider(string fileConfigJson = "mappings.json")
        {
            try
            {
                totalTicker = TotalTicker.Instance();
                var mappedSymbols = ConfigurationManager.GetJson(fileConfigJson);
                initializeMapping(mappedSymbols);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        protected static ConcurrentDictionary<string, ProviderConfig> _mappings = new ConcurrentDictionary<string, ProviderConfig>();

        protected string ProviderId { get; set; }

        public List<string> Symbol { get; protected set; }

        public string ProviderName{ get; protected set; }

        abstract public Uri GetUri();

        abstract public void OnMessage(string data);

        private static void initializeMapping(List<ObjJsonConfig> mappedSymbols)
        {
            foreach (var mappedSymbol in mappedSymbols)
            {
                foreach (var provider in mappedSymbol.Providers)
                {
                    //if (_mappings.Keys.ToList().Find(s => s == provider.ProviderId) == null)
                    if (!_mappings.ContainsKey(provider.ProviderId))
                        _mappings[provider.ProviderId] = new ProviderConfig();

                    var symbolConfig = new SymbolConfig()
                    {
                        Symbol = mappedSymbol.Symbol,
                        ProviderId = provider.ProviderId,
                        SymbolApi = provider.mappedTo
                    };
                    _mappings[provider.ProviderId].addSymbolConfig(symbolConfig);
                }
            }
        }

        protected bool processTicker(BaseTicker ticker)
        {
            // true - тікер обробили, його потрібно вивести
            // false - тікер не змінився
            bool processed = false;
            if(ticker.Symbol != string.Empty && ticker.BidPrice != 0 && ticker.AskPrice !=0 && ticker.TotalTradedVolume != 0)
            {
                if(totalTicker.dictionaryTicker.ContainsKey(ticker.Symbol))
                {
                    BaseTicker updatedTicker;
                    processed = calculateTicker(ticker, out updatedTicker);
                    if (processed)
                    {
                        totalTicker.dictionaryTicker[ticker.Symbol] = updatedTicker;
                    }
                }
                else
                {
                    totalTicker.dictionaryTicker[ticker.Symbol] = ticker;
                    processed = true;
                    //totalTicker.dictionaryTicker.TryAdd(ticker.Symbol, ticker);
                }
            }
            return processed;
        }

        protected bool calculateTicker(BaseTicker newTicker, out BaseTicker resultTicker)
        {
            // true - прийшов новий тікер
            // false - тікер рівний тікеру в dictionaryTicker
            bool changed = false;
            BaseTicker lastTicker = totalTicker.dictionaryTicker[newTicker.Symbol];
            if (lastTicker.AskPrice != newTicker.AskPrice || 
                lastTicker.BidPrice != newTicker.BidPrice || 
                lastTicker.TotalTradedVolume != newTicker.TotalTradedVolume)
            {
                resultTicker = newTicker;
                changed = true;
            }
            else resultTicker = new BaseTicker();
            
            return changed;
        }
    }
}
