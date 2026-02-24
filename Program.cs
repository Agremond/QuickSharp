using QuikSharp.DataStructures;
using QuikSharp.Transports;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    internal static class Program
    {
        private static Quik _quik;
        private static bool _exitSystem;

        private static async Task Main()
        {
            string securityCode = "RIZ5";

            Console.WriteLine("Starting QuikSharp 2026...");

            // Создаём транспорт (пример — SHM)
            IQuikTransport transport = new ShmQuikTransport(); // <-- твоя реализация

            _quik = new Quik(transport);

            await _quik.ConnectAsync();

            Console.WriteLine("Connected.");

            _quik.Events.OnStop += signal =>
            {
                Console.WriteLine("OnStop: " + signal);
                _exitSystem = true;
            };

            _quik.Events.OnClose += () =>
            {
                Console.WriteLine("OnClose");
                _exitSystem = true;
            };

            var serverTime = await _quik.Service.GetInfoParam(InfoParams.SERVERTIME);
            Console.WriteLine("Server time: " + serverTime);

            #region Quote Example

            double bestBidPrice = 0;
            double bestOfferPrice = 0;

            _quik.Events.OnQuote += orderBook =>
            {
                if (orderBook.sec_code == securityCode)
                {
                    bestBidPrice = orderBook.bid.Max(o => o.price);
                    bestOfferPrice = orderBook.offer.Min(o => o.price);
                }
            };

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 200; i++)
            {
                Console.WriteLine($"Best bid: {bestBidPrice} | Best offer: {bestOfferPrice}");

                var time = await _quik.Service.GetInfoParam(InfoParams.SERVERTIME);

                await Task.Delay(50);
            }

            sw.Stop();

            Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Per call: {sw.ElapsedMilliseconds / 200} ms");

            #endregion

            Console.WriteLine("Waiting for exit...");

            while (!_exitSystem)
                await Task.Delay(100);

            Cleanup();
        }

        private static void Cleanup()
        {
            Console.WriteLine("Shutting down...");
            _quik?.Dispose();
        }
    }
}