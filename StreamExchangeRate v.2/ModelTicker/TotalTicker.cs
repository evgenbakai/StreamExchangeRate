using System;
using System.Collections.Concurrent;

namespace StreamExchangeRate_v._2
{
    class TotalTicker
    {
        public ConcurrentDictionary<string, BaseTicker> dictionaryTicker = new ConcurrentDictionary<string, BaseTicker>();

        static TotalTicker uniqueInstance;

        public static TotalTicker Instance()
        {
            if (uniqueInstance == null)
                uniqueInstance = new TotalTicker();

            return uniqueInstance;
        }
        protected TotalTicker()
        {
        }
    }
}
