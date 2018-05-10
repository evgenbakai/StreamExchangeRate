using System;
using System.Collections.Generic;

namespace StreamExchangeRate_v._2.Config
{
    public class ProviderConfig
    {
        public List<SymbolConfig> _symbols = new List<SymbolConfig>();

        public void addSymbolConfig(SymbolConfig config)
        {
           // if(!_symbols.Exists(s => s.SymbolApi == config.SymbolApi))
            _symbols.Add(config);
        }

        public SymbolConfig getSymbolConfig(string apiSymbol)
        {
            return _symbols.Find(s => s.SymbolApi == apiSymbol);
        }

        public string getSymbol(string apiSymbol)
        {
            return getSymbolConfig(apiSymbol).Symbol;
        }

        public List<string> getListSymbolApi()
        {
            List<string> listResult = new List<string>();
            _symbols.ForEach(s => listResult.Add(s.SymbolApi));
            return listResult;
        }
        public List<string> getListSymbol()
        {
            List<string> listResult = new List<string>();
            _symbols.ForEach(s => listResult.Add(s.Symbol));
            return listResult;
        }
    }
}