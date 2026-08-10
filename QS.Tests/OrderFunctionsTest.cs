using NUnit.Framework;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Tests
{
    [TestFixture]
    [Category("OrderFunctions")]
    public class OrderFunctionsTests
    {
        private readonly Quik _quik;
        private readonly OrderFunctions _orders;

        // Можно передать через конструктор (если используете TestCaseSource или SetUpFixture),
        // либо через [OneTimeSetUp], если Quik инициализируется один раз на все тесты
        public OrderFunctionsTests(Quik quik)
        {
            _quik = quik ?? throw new ArgumentNullException(nameof(quik));
            _orders = quik.Orders; // предполагается, что у Quik есть свойство Orders
        }

        #region GetOrder

        [Test]
        [Description("Получение несуществующей заявки должно возвращать null или Order с соответствующим состоянием")]
        public async Task GetOrder_NonExistentOrder()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            // Arrange
            const string classCode = "TQBR";
            const long nonExistentOrderId = 999999999999;

            // Act
            Order? order = await _orders.GetOrder(classCode, nonExistentOrderId);

            // Assert
            Assert.That(order, Is.Null.Or.Property(nameof(Order.OrderNum)).EqualTo(0));
        }

        [Test]
        [Description("Получение существующей заявки по номеру")]
        [TestCase("TQBR", 77906433305)] // Замените на реальный номер заявки из вашего терминала
        public async Task GetOrder_ExistingOrder(string classCode, long orderNum)
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            // Act
            Order? order = await _orders.GetOrder(classCode, orderNum);

            // Assert
            Assert.That(order, Is.Not.Null, "Заявка должна существовать");
            Assert.That(order.OrderNum, Is.EqualTo(orderNum));
            Assert.That(order.ClassCode, Is.EqualTo(classCode));
            Assert.That(order.State, Is.Not.EqualTo(0), "Состояние заявки должно быть заполнено");

            Console.WriteLine($"Order {orderNum}: State = {order.State}, Qty = {order.Quantity}, Price = {order.Price}");
        }

        #endregion

        #region GetOrders

        [Test]
        public async Task GetAllOrders()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            List<Order> orders = await _orders.GetOrders();

            Assert.That(orders, Is.Not.Null);
            Console.WriteLine($"Всего заявок в терминале: {orders.Count}");

            // Опционально: можно проверить, что хотя бы некоторые поля заполнены
            if (orders.Any())
            {
                var first = orders.First();
                Assert.That(first.ClassCode, Is.Not.Null.Or.Empty);
            }
        }

        //[Test]
        //[TestCase("TQBR", "SBER")]
        //[TestCase("TQBR", "GAZP")]
        //public async Task GetOrders_ByClassAndSecCode(string classCode, string secCode)
        //{
        //    List<Order> orders = await _orders.GetOrders(classCode, secCode);

        //    Assert.That(orders, Is.Not.Null);
        //    Console.WriteLine($"Заявок по {classCode}|{secCode}: {orders.Count}");

        //    foreach (var order in orders)
        //    {
        //        Assert.That(order.ClassCode, Is.EqualTo(classCode));
        //        Assert.That(order.SecCode, Is.EqualTo(secCode));
        //    }
        //}

        #endregion

        #region GetOrder_by_transID

        [Test]
        public async Task GetOrderByTransID()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            Order? order = await _orders.GetOrder_by_transID("TQBR", "SBER", 999999999);

            Assert.That(order, Is.Null);
        }

        #endregion

        #region CreateOrder и KillOrder (осторожно!)

        /// <summary>
        /// Этот тест создаёт реальную заявку! Используйте только в тестовом окружении 
        /// с минимальным лотом и очень осторожно.
        /// </summary>
        [Test]
        [Explicit("Создаёт реальную заявку. Запускать только вручную!")]
        [Category("LiveTrading")]
        public async Task CreateAndKillOrder()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            // Arrange — очень маленькая заявка (например, 1 лот лимитки далеко от рынка)
            var newOrder = new Order
            {
                Account = "L01-00000F00",        // ← подставьте свой счёт
                ClassCode = "TQBR",
                SecCode = "SBER",
                Operation = Operation.Buy,
                Quantity = 1,
                Price = 100.0m,                  // сильно ниже рынка, чтобы не исполнилась
                ClientCode = "TestClient",
                ExecType = 0                     // PUT_IN_QUEUE
            };

            // Act
            long transResult = await _orders.CreateOrder(newOrder);

            Assert.That(transResult, Is.GreaterThan(0), "Транзакция должна быть принята");

            // Даём время на регистрацию заявки
            await Task.Delay(1500);

            // Получаем заявку по transID (если нужно) или ищем по другим методам

            // Kill
            long killResult = await _orders.KillOrder(newOrder);

            Assert.That(killResult, Is.GreaterThanOrEqualTo(0));
        }

        #endregion

        #region Helper methods (опционально)

        private async Task<Order?> FindAnyActiveOrder(string classCode = "TQBR")
        {
            var allOrders = await _orders.GetOrders();
            return allOrders.FirstOrDefault(o =>
                o.ClassCode == classCode &&
                (o.State == State.Completed || o.State == State.Active)); // активные/частично исполненные
        }

        #endregion
    }
}