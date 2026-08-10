using NUnit.Framework;
using QuikSharp;
using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Tests
{
    [TestFixture]
    public class TradingFunctionsTest
    {
        private readonly Quik _q;

        public TradingFunctionsTest(Quik quik)
        {
            _q = quik ?? throw new ArgumentNullException(nameof(quik));
        }

        // -------------------------------
        // Helper
        // -------------------------------

        private void Log(string name)
        {
            Console.WriteLine($"\n=== {name} ===");
        }

        // -------------------------------
        // JSON / OrderBook
        // -------------------------------

        [Test]
        public void CouldDeserializeOrderBook()
        {
            Log(nameof(CouldDeserializeOrderBook));

            var ob = _orderBookSample.FromJson<OrderBook>();

            Assert.IsNotNull(ob);
            Assert.IsNotNull(ob.bid);
            Assert.IsNotNull(ob.offer);

            Console.WriteLine("Order book OK");
        }

        // -------------------------------
        // Депо лимиты
        // -------------------------------

        [Test]
        public void GetDepoLimitsTest()
        {
            Log(nameof(GetDepoLimitsTest));

            var depoLimits = _q.Trading.GetDepoLimits().Result;

            Assert.IsNotNull(depoLimits);
            Console.WriteLine($"Всего лимитов: {depoLimits.Count}");

            if (depoLimits.Any())
                PrintDepoLimits(depoLimits);

            // Проверка по инструменту (если есть)
            var sec = depoLimits.FirstOrDefault()?.SecCode;

            if (!string.IsNullOrEmpty(sec))
            {
                var filtered = _q.Trading.GetDepoLimits(sec).Result;

                Assert.IsNotNull(filtered);
                Console.WriteLine($"Лимиты по {sec}: {filtered.Count}");
            }
        }

        [Test]
        public void GetDepoTest()
        {
            Log(nameof(GetDepoTest));

            var depoLimits = _q.Trading.GetDepoLimits().Result;
            var sample = depoLimits.FirstOrDefault();

            if (sample == null)
            {
                Assert.Inconclusive("Нет данных для теста");
                return;
            }

            var depo = _q.Trading.GetDepo(
                sample.ClientCode,
                sample.FirmId,
                sample.SecCode,
                sample.TrdAccId).Result;

            Assert.IsNotNull(depo);
            Console.WriteLine(depo.ToJson());
        }

        // -------------------------------
        // Денежные лимиты
        // -------------------------------

        [Test]
        public void GetMoneyLimitsTest()
        {
            Log(nameof(GetMoneyLimitsTest));

            var limits = _q.Trading.GetMoneyLimits().Result;

            Assert.IsNotNull(limits);
            Console.WriteLine($"MoneyLimits: {limits.Count}");

            if (limits.Any())
                Console.WriteLine(limits.First().ToJson());
        }

        // -------------------------------
        // Фьючерсы
        // -------------------------------

        [Test]
        public void GetFuturesClientLimitsTest()
        {
            Log(nameof(GetFuturesClientLimitsTest));

            var limits = _q.Trading.GetFuturesClientLimits().Result;

            Assert.IsNotNull(limits);
            Console.WriteLine($"FuturesLimits: {limits.Count}");
        }

        [Test]
        public void GetFuturesClientHoldingsTest()
        {
            Log(nameof(GetFuturesClientHoldingsTest));

            var holdings = _q.Trading.GetFuturesClientHoldings().Result;

            Assert.IsNotNull(holdings);
            Console.WriteLine($"Holdings: {holdings.Count}");
        }

        // -------------------------------
        // Параметры
        // -------------------------------

        [Test]
        public void GetParamExTest()
        {
            Log(nameof(GetParamExTest));

            var param = _q.Trading.GetParamEx("TQBR", "SBER", ParamNames.LAST).Result;

            Assert.IsNotNull(param);
            Console.WriteLine(param.ToJson());
        }

        [Test]
        public void ParamRequestTest()
        {
            Log(nameof(ParamRequestTest));

            bool ok = _q.Trading.ParamRequest("TQBR", "SBER", ParamNames.LAST).Result;

            Assert.IsTrue(ok);

            var cancel = _q.Trading.CancelParamRequest("TQBR", "SBER", ParamNames.LAST).Result;

            Assert.IsTrue(cancel);
        }

        // -------------------------------
        // Сделки
        // -------------------------------

        [Test]
        public void GetTradesTest()
        {
            Log(nameof(GetTradesTest));

            var trades = _q.Trading.GetTrades().Result;

            Assert.IsNotNull(trades);
            Console.WriteLine($"Trades: {trades.Count}");
        }

        [Test]
        public void GetAllTradesTest()
        {
            Log(nameof(GetAllTradesTest));

            var trades = _q.Trading.GetAllTrades().Result;

            Assert.IsNotNull(trades);
            Console.WriteLine($"AllTrades: {trades.Count}");
        }

        // -------------------------------
        // Портфель
        // -------------------------------

        [Test]
        public void GetPortfolioInfoTest()
        {
            Log(nameof(GetPortfolioInfoTest));

            var depo = _q.Trading.GetDepoLimits().Result.FirstOrDefault();

            if (depo == null)
            {
                Assert.Inconclusive("Нет данных");
                return;
            }

            var portfolio = _q.Trading.GetPortfolioInfo(depo.FirmId, depo.ClientCode).Result;

            Assert.IsNotNull(portfolio);
            Console.WriteLine(portfolio.ToJson());
        }

        // -------------------------------
        // Utils
        // -------------------------------

        private void PrintDepoLimits(List<DepoLimitEx> depoLimits)
        {
            foreach (var depo in depoLimits.Take(5)) // ограничим вывод
            {
                Console.WriteLine($"[{depo.SecCode}] {depo.CurrentBalance} @ {depo.TrdAccId}");
            }
        }
        // -------------------------------
        // GetOptionBoard — РАСШИРЕННЫЙ ТЕСТ ДЛЯ РАЗНЫХ СЕРИЙ
        // -------------------------------

        [TestCase("SiM6", "0", "Неделя (ближайшая)")]
        [TestCase("SiM6", "1", "Месяц (ближайший)")]
        [TestCase("SiM6", "4", "Все серии")]
        [TestCase("SiH6", "1", "Месяц для SiH6")]
        public async Task GetOptionBoard_DifferentSeries(string secCode, string series, string description)
        {
            Log($"{nameof(GetOptionBoard_DifferentSeries)} — {description}");

            string classCode = "SPBOPT";

            Console.WriteLine($"Запрос: Class={classCode}, Sec={secCode}, Series={series} ({description})");

            var result = await _q.Trading.GetOptionBoard(classCode, secCode, series);

            Assert.IsNotNull(result, "GetOptionBoard вернул null");

            Console.WriteLine($"Получено опционов: {result.Count}");

            if (result.Count == 0)
            {
                Assert.Inconclusive($"Для {secCode} series={series} не вернулось ни одного опциона");
                return;
            }

            // Статистика
            int calls = result.Count(o => o.OPTIONTYPE?.ToLowerInvariant() == "call");
            int puts = result.Count(o => o.OPTIONTYPE?.ToLowerInvariant() == "put");
            var uniqueStrikes = result.Select(o => o.Strike).Distinct().Count();

            Console.WriteLine($"Call: {calls} | Put: {puts} | Уникальных страйков: {uniqueStrikes}");

            // Вывод таблицы (первые 12 + последние 5)
            PrintOptionBoard(result, maxItems: 12, showLast: 5);

            // Дополнительные проверки
            Assert.Greater(calls + puts, 0, "Не найдено ни Call, ни Put");
            Assert.Greater(uniqueStrikes, 5, "Слишком мало уникальных страйков");

            Console.WriteLine($"Тест для серии {series} ({description}) — УСПЕШНО");
        }

        // -------------------------------
        // Улучшенный метод печати с возможностью показать последние строки
        // -------------------------------
        private void PrintOptionBoard(List<OptionBoard> options, int maxItems = 12, int showLast = 5)
        {
            if (options == null || options.Count == 0)
            {
                Console.WriteLine("Нет данных для отображения.");
                return;
            }

            Console.WriteLine("\n" + new string('─', 130));

            Console.WriteLine(
                $"{"Strike",-8} {"Type",-6} {"Code",-14} " +
                $"{"Bid",-10} {"Ask",-10} {"Last",-10} " +
                $"{"Theo",-10} {"IV",-8} " +
                $"{"DTE",-6} {"ExpDate",-12} " +
                $"{"Lot",-6} {"Step",-8} {"StepVal",-10}"
            );
            Console.WriteLine(new string('─', 130));

            // Первые N
            for (int i = 0; i < Math.Min(maxItems, options.Count); i++)
            {
                PrintOptionRow(options[i]);
            }

            // Если много — показываем пропуск
            if (options.Count > maxItems + showLast)
            {
                Console.WriteLine($"{"...",-8} {"...",-6} {"...",-14} {"...",-10} {"...",-10} {"...",-10} {"...",-8} {"...",-8} {"...",-6} {"...",-12}");
            }

            // Последние N
            int startLast = Math.Max(maxItems, options.Count - showLast);
            for (int i = startLast; i < options.Count; i++)
            {
                PrintOptionRow(options[i]);
            }

            Console.WriteLine(new string('─', 130));
        }

        private void PrintOptionRow(OptionBoard opt)
        {
            string type = opt.OPTIONTYPE?.ToUpperInvariant() switch
            {
                "CALL" => "CALL",
                "PUT" => "PUT ",
                _ => opt.OPTIONTYPE?.PadRight(6) ?? "????"
            };

            Console.WriteLine(
                $"{opt.Strike,-8} " +
                $"{type,-6} " +
                $"{opt.Code,-14} " +

                $"{opt.BID,-10:F2} " +
                $"{opt.OFFER,-10:F2} " +
                $"{opt.LastPrice,-10:F2} " +

                $"{opt.TheorPrice,-10:F2} " +
                $"{opt.Volatility,-8:F2} " +

                $"{opt.DAYSTOMATDATE,-6} " +
                $"{opt.ExpDate,-12} " +

                $"{opt.Lot,-6} "

            );
        }

        private readonly string _orderBookSample = @"{""bid_count"":""1"",""offer_count"":""1"",""bid"":[{""price"":""1"",""quantity"":""1""}],""offer"":[{""price"":""2"",""quantity"":""1""}]}";
    }
}