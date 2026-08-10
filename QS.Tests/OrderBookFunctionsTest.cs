using NUnit.Framework;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;

namespace QS.Tests {
    [TestFixture]
    public class OrderBookFunctionsTest {
        private readonly Quik _q;

        public OrderBookFunctionsTest(Quik quik) {
            _q = quik ?? throw new ArgumentNullException(nameof(quik));
        }

        [Test]
        public void Subscribe_Level_II_Quotes() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            bool result = _q.OrderBook.Subscribe("SPBFUT", "RIM6").Result;
            if (result)
            {
                Console.WriteLine("Subscribe_Level_II_Quotes: "
                + String.Join(",", result));
                _q.Events.OnQuote += OnQuoteDo;
            //    _q.Events.OnParam += OnParamDo;
            }
            

        }

        [Test]
        void OnQuoteDo(OrderBook quote)
        {
            Console.WriteLine($"Пришла котировка: {quote.sec_code}, Bid count: {quote.bid_count}, Ask count: {quote.offer_count}");
        }
        [Test]
        void OnParamDo(Param param)
        {
          //  Console.WriteLine($"Пришло OnParam: {param.}");
        }

        [Test]
        public void Unsubscribe_Level_II_Quotes() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Console.WriteLine("Unsubscribe_Level_II_Quotes: "
                + String.Join(",", _q.OrderBook.Unsubscribe("SPBFUT", "RIM6").Result));
        }

        [Test]
        public void IsSubscribed_Level_II_Quotes() {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Console.WriteLine("IsSubscribed_Level_II_Quotes: "
                + String.Join(",", _q.OrderBook.IsSubscribed("SPBFUT", "RIH5").Result));
        }


    }
}
