using System;
using StreamExchangeRate.Binance;
using System.Threading;
using System.Collections.Generic;

namespace StreamExchangeRate
{
    class Program
    {
        static void Main(string[] args)
        {
/*
            // --- Test
            // create list providers
            List<Provider> list_providers = new List<Provider>();
            for (int i = 0; i < 1000; i++)
            {
                // try - catch
                list_providers.Add(new BinanceClient("Binance"));
            }

            // connect to web socket
            foreach(Provider p in list_providers)
            {
                p.ConnectAsync();
            }
            
            // task in main thread
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine("i = " + i);
                Thread.Sleep(1000);
            }
*/

            BinanceClient binance = null;
            try
            {
  
                binance = new BinanceClient("Binance");
                binance.ConnectAsync();   // Start
            }
            catch (Exception ex)
            {
                Console.WriteLine("BinanceClient: " + ex.Message);
            }

            // test
            Thread.Sleep(5000);
            binance.DisconnectAsync();    // Stop

            Console.ReadKey(true);
        }
    }
}
