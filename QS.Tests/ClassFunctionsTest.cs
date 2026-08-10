using NUnit.Framework;
using QuikSharp;
using System;
using System.Linq;

namespace QS.Tests
{
    [TestFixture]
    public class ClassFunctionsTest
    {
        private readonly Quik _q;

        public ClassFunctionsTest(Quik quik)
        {
            _q = quik ?? throw new ArgumentNullException(nameof(quik));
        }

        [Test]
        public void GetClassesList()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var classes = _q.Class.GetClassesList().Result;

            Assert.IsNotNull(classes);
            Assert.IsNotEmpty(classes);

            Console.WriteLine("GetClassesList: " + String.Join(",", classes));
        }

        [Test]
        public void GetClassInfo()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var list = _q.Class.GetClassesList().Result;

            foreach (var s in list)
            {
                var info = _q.Class.GetClassInfo(s).Result;

                Assert.IsNotNull(info);

                Console.WriteLine($"GetClassInfo for {s}: {info.Name}");
            }
        }

        [Test]
        public void GetClassSecurities()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var list = _q.Class.GetClassesList().Result;

            foreach (var s in list)
            {
                var securities = _q.Class.GetClassSecurities(s).Result;

                Assert.IsNotNull(securities);

                Console.WriteLine($"GetClassSecurities for {s}: {securities.Length}");
            }
        }

        [Test]
        public void GetSecurityInfo()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var sec1 = _q.Class.GetSecurityInfo("SPBFUT", "RIM6").Result;
            var sec2 = _q.Class.GetSecurityInfo("TQBR", "LKOH").Result;

            Assert.IsNotNull(sec1);
            Assert.IsNotNull(sec2);

            Console.WriteLine("RIM6: " + sec1.ToJson());
            Console.WriteLine("LKOH: " + sec2.ToJson());
        }

        [Test]
        public void GetSecurityClass()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var classes = _q.Class.GetClassesList().Result;
            var classList = String.Join(",", classes);

            var secClass = _q.Class.GetSecurityClass(classList, "LKOH").Result;

            Assert.IsNotNull(secClass);

            Console.WriteLine($"Security class for LKOH: {secClass}");
        }

        [Test]
        public void GetClientCode()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var clientCode = _q.Class.GetClientCode().Result;

            Assert.IsNotNull(clientCode);

            Console.WriteLine($"ClientCode: {clientCode}");
        }

        [Test]
        public void GetClientCodes()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var clientCodes = _q.Class.GetClientCodes().Result;

            Assert.IsNotNull(clientCodes);
            Assert.IsNotEmpty(clientCodes);

            Console.WriteLine("ClientCodes: " + String.Join(",", clientCodes));
        }

        [Test]
        public void GetTradeAccount()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var account = _q.Class.GetTradeAccount("TQBR").Result;

            Assert.IsNotNull(account);

            Console.WriteLine($"TradeAccount (TQBR): {account}");
        }

        [Test]
        public void GetTradeAccounts()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);
            var accounts = _q.Class.GetTradeAccounts().Result;

            Assert.IsNotNull(accounts);
            Assert.IsNotEmpty(accounts);

            foreach (var acc in accounts)
            {
                Console.WriteLine($"Account: {acc.TrdaccId}");
            }
        }
    }
}