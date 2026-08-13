// Программа для запуска интеграционных тестов QuikSharp
// Использует один общий экземпляр Quik + SHM-транспорт для всех тестов

using QS.Tests;           // предполагается, что здесь находятся все *Test-классы
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.Transports;
using System;
using System.Threading.Tasks;

namespace QuikSharp.IntegrationTests
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("=== Интеграционные тесты QuikSharp ===\n");

            // ────────────────────────────────────────────────────────────────
            // 1. Создание единственного транспорта и клиента Quik
            // ────────────────────────────────────────────────────────────────
            var (kind, host, responsePort, callbackPort) = TransportFactory.ReadFromDefaultConfig();
            Console.WriteLine($"Инициализация транспорта: {kind}...");
            IQuikTransport transport = TransportFactory.Create(kind, host, responsePort, callbackPort);

            Console.WriteLine("Создание клиента Quik...");
            var quik = new Quik(transport);

            // Подписка на ключевые события (можно раскомментировать при отладке)
             quik.Events.OnConnected    += () => Console.WriteLine("→ Подключено к QUIK");
             quik.Events.OnDisconnected += () => Console.WriteLine("→ Отключено от QUIK");
             //quik.Events.OnError        += msg => Console.WriteLine($"Ошибка QUIK: {msg}");

            // ────────────────────────────────────────────────────────────────
            // 2. Подключение к QUIK
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("Подключение к QUIK...");
            try
            {
                await quik.ConnectAsync();
                await Task.Delay(1500); // даём время на инициализацию коллбэков и SHM
                Console.WriteLine("Подключение успешно.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                transport.Dispose();
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            // ────────────────────────────────────────────────────────────────
            // 3. Запуск тестов (все используют один и тот же quik)
            // ────────────────────────────────────────────────────────────────
            try
            {
                //RunClassTests(quik);
                //RunServiceTests(quik);
               // RunTradingTests(quik);
                //RunCandleFunctionsTests(quik);
                RunOrderFunctionsTests(quik);
               //RunOrderBookTests(quik);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nКритическая ошибка во время тестов: {ex}");
            }

            // ────────────────────────────────────────────────────────────────
            // 4. Завершение
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("\nВсе тесты завершены.");

            // Отключение и очистка ресурсов
           
            
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
            transport.Dispose();
            Console.WriteLine("Ресурсы освобождены.");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }


        private static void RunClassTests(Quik quik)
        {
            Console.WriteLine("=== Тесты классов и инструментов ===");

            var test = new ClassFunctionsTest(quik);

            try
            {
                test.GetClassesList();
                test.GetClassInfo();
                test.GetClassSecurities();
                test.GetSecurityInfo();

                test.GetSecurityClass();
                test.GetClientCode();
                test.GetClientCodes();
                test.GetTradeAccount();
                test.GetTradeAccounts();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при выполнении тестов: " + ex.Message);
            }

            Console.WriteLine();
        }
        private static void RunServiceTests(Quik quik)
        {
            Console.WriteLine("=== Сервисные тесты ===");
            var test = new ServiceFunctionsTest(quik);

            test.IsConnected();         // опечатка исправлена: IsConencted → IsConnected
            test.GetWorkingFolder();
            test.GetScriptPath();
            test.GetInfoParam();

            // test.Message();          // закомментировано, т.к. требует проверки

            Console.WriteLine();
        }

        private static async Task SafeRun(Action action)
        {
            try
            {
                action();
                Console.WriteLine("OK\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("Возможные причины:");
                Console.WriteLine("- Не открыты таблицы в QUIK");
                Console.WriteLine("- Нет данных по инструменту");
                Console.WriteLine("- Lua-скрипт не запущен");
                Console.WriteLine();
            }

            await Task.Delay(200); // небольшая пауза (QUIK иногда лагает)
        }
        private static async Task RunTradingTests(Quik quik)
        {
            Console.WriteLine("=== Торговые тесты ===");

            var test = new TradingFunctionsTest(quik);



            await SafeRun(() =>
            {
                Console.WriteLine("Тест: DepoLimits");
                test.GetDepoLimitsTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: DepoLimits");
                test.GetOptionBoard_DifferentSeries("SiM6", "0", "Неделя (ближайшая)");
                //test.GetOptionBoard_DifferentSeries("SiM6", "1", "Неделя (ближайшая)");
                //test.GetOptionBoard_DifferentSeries("SiM6", "4", "Неделя (ближайшая)");
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: GetDepo");
                test.GetDepoTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: MoneyLimits");
                test.GetMoneyLimitsTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: Futures Limits");
                test.GetFuturesClientLimitsTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: Futures Holdings");
                test.GetFuturesClientHoldingsTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: ParamEx");
                test.GetParamExTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: ParamRequest");
                test.ParamRequestTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: Trades");
                test.GetTradesTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: AllTrades");
                test.GetAllTradesTest();
            });

            await SafeRun(() =>
            {
                Console.WriteLine("Тест: Portfolio");
                test.GetPortfolioInfoTest();
            });

            Console.WriteLine("\n=== Торговые тесты завершены ===\n");
        }

        private static async Task RunOrderFunctionsTests(Quik quik)
        {
            Console.WriteLine("=== Тесты OrderFunctions (Заявки) ===");
            Console.WriteLine($"Время запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            var test = new OrderFunctionsTests(quik);

             //SafeRun(() => test.GetOrder_NonExistentOrder());
             //SafeRun(() => test.GetOrder_ExistingOrder("TQBR", 14278245258));
             SafeRun(() => test.GetAllOrders());
             SafeRun(() => test.GetOrderByTransID());


            Console.WriteLine("\n=== Тесты OrderFunctions завершены ===\n");
        }
        private static async Task RunCandleFunctionsTests(Quik quik)
        {
            Console.WriteLine("=== Тесты CandleFunctions (Свечи) ===");
            Console.WriteLine($"Время запуска: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

            var test = new CandleFunctionsTest(quik);
            SafeRun(() => test.GetAllCandlesTest());
            SafeRun(() => test.GetLastCandlesTest());


            Console.WriteLine("\n=== Тесты CandleFunctions завершены ===\n");
        }

        private static void RunOrderBookTests(Quik quik)
        {
            Console.WriteLine("=== Тесты стакана (Level II) ===");
            var test = new OrderBookFunctionsTest(quik);

            test.Subscribe_Level_II_Quotes();
            Task.Delay(20000);
            //test.Unsubscribe_Level_II_Quotes() ;
            Console.WriteLine();
        }


       
    }
}