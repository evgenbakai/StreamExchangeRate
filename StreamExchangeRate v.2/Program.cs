using StreamExchangeRate_v._2.Binance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamExchangeRate_v._2
{
    class Program
    {
        static void Main(string[] args)
        {

            Provider p = new Provider(new BinanceClient("24"));
            p.Start();

            /*
            List<Provider> list_providers = new List<Provider>();
            for (int i = 0; i < 1000; i++)
            {
                list_providers.Add(new Provider(new BinanceClient("Binance")));
            }
            foreach (Provider p in list_providers)
            {
                p.Start();
            }
            */
            Console.ReadKey(true);
        }
    }
}
