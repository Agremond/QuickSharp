using NUnit.Framework;
using QuikSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using QuikSharp.DataStructures;
using QuikSharp.Transports;

namespace QS.Tests
{
    [TestFixture]
    public class CandleFunctionsTest
    {
        private readonly Quik _q;

        public CandleFunctionsTest(Quik quik)
        {
            _q = quik ?? throw new ArgumentNullException(nameof(quik));
        }

        [Test]
               
        public void GetAllCandlesTest()
        {

            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            //Получаем месячные свечки по инструменту "Северсталь"
            List<Candle> candles = _q.Candles.GetAllCandles("TQBR", "YDEX", CandleInterval.MN).Result;
            Trace.WriteLine("Candles count: " + candles.Count);
        }

        [Test]
        public void GetLastCandlesTest()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            int Days = 7;
            List<Candle> candles = _q.Candles.GetLastCandles("TQBR", "YDEX", CandleInterval.D1, Days).Result;


            Days = 77;
            candles = _q.Candles.GetLastCandles("TQBR", "YDEX", CandleInterval.D1, Days).Result;
  

            Days = 1;
            candles = _q.Candles.GetLastCandles("TQBR", "YDEX", CandleInterval.D1, Days).Result;

        }

        [Test]
        public void CandlesSubscriptionTest()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            _q.Candles.NewCandle += OnNewCandle;

			// На всякий случай вначале нужно отписатся (иначе может вылететь Assert)
			// TODO: Вообще у библиотеки огромная проблема - Lua скрипт не отписывается от того к чему он подписался при отключении клиента.
			// В результате при следующем подключении клиент начинает получать сразу кучу CallBack'ов, на которые он не подписывался в текущей сессии.
			// По большому счету сейчас клиент должен сам заботаться о том, что бы гарантированно отписываться от всего к чему подписался при выходе.
            bool isSubscribed = _q.Candles.IsSubscribed("TQBR", "YDEX", CandleInterval.M1).Result;
			if (isSubscribed)
                _q.Candles.Unsubscribe ("TQBR", "YDEX", CandleInterval.M1).Wait ();

			// Проверяем что мы действительно отписались
			isSubscribed = _q.Candles.IsSubscribed ("TQBR", "YDEX", CandleInterval.M1).Result;


            _q.Candles.Subscribe("TQBR", "YDEX", CandleInterval.M1).Wait ();
            isSubscribed = _q.Candles.IsSubscribed("TQBR", "YDEX", CandleInterval.M1).Result;


            // Раскомментарить если необходимо получать данные в функции OnNewCandle 2 минуты. В течении этих двух минут должна прийти еще одна свечка
           // Thread.Sleep(120000);//must get at leat one candle as use minute timeframe

            _q.Candles.Unsubscribe("TQBR", "YDEX", CandleInterval.M1).Wait ();
			isSubscribed = _q.Candles.IsSubscribed("TQBR", "YDEX", CandleInterval.M1).Result;
		


		}

		private void OnNewCandle(Candle candle)
        {
            if (candle.SecCode == "YDEX" && candle.ClassCode == "TQBR" && candle.Interval == CandleInterval.M1)
            {
                Console.WriteLine("Sec:{0}, Open:{1}, Close:{2}, Volume:{3}", candle.SecCode, candle.Open, candle.Close, candle.Volume);
            }
        }



    }
}
