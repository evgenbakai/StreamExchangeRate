using StreamExchangeRate_v._2.Binance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamExchangeRate_v._2
{
    class Program
    {
        static void Main(string[] args)
        {
            Provider ws = new Provider(new BinanceClient("Binance"));
            ws.Start();


            Console.ReadKey(true);
        }
    }
}
