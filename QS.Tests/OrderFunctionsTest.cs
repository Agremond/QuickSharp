using NUnit.Framework;
using QuikSharp;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;

namespace QS.Tests
{
    [TestFixture]
    public class OrderFunctionsTest
    {
        IQuikTransport transport = new ShmQuikTransport();
        [Test]
        public void GetOrderTest()
        {
            Quik quik = new Quik(transport);

            //Заведомо не существующая заявка.
            long orderId = 123456789;
            Order order = quik.Orders.GetOrder("TQBR", orderId).Result;
         //   Assert.IsNull(order);

            //Заявка с таким номером должна присутствовать в таблице заявок.
            orderId = 14278245258;//вставьте свой номер
            order = quik.Orders.GetOrder("TQBR", orderId).Result;
            if (order != null)
            {
                Console.WriteLine("Order state: " + order.State);
            }
            else
            {
                Console.WriteLine("Order doesn't exsist.");
            }
        }
    }
}
