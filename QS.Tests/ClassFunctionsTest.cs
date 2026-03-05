using NUnit.Framework;
using QuikSharp;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;


namespace QS.Tests {

    [TestFixture]
    public class ClassFunctionsTest {
        public ClassFunctionsTest() {
            _q = new Quik(transport);
            _isQuik = _q.Debug.IsQuik().Result;
        }
       
        // Создаем транспорт (SHM)
        IQuikTransport transport = new ShmQuikTransport();
        private Quik _q;
        private bool _isQuik;
        
        
        [Test]
        public void GetClassesList() {

            Console.WriteLine("GetClassesList: "
                + String.Join(",", _q.Class.GetClassesList().Result));
        }

        [Test]
        public void GetClassInfo() {
            var list = _q.Class.GetClassesList().Result;
            foreach (var s in list) {
                Console.WriteLine("GetClassInfo for " + s + ": "
                + String.Join(",", _q.Class.GetClassInfo(s).Result));
            }
        }



        [Test]
        public void GetClassSecurities() {
            var list = _q.Class.GetClassesList().Result;
            foreach (var s in list) {
                Console.WriteLine("GetClassSecurities for " + s + ": "
                + String.Join(",", _q.Class.GetClassSecurities(s).Result));
            }
        }

        [Test]
        public void GetSecurityInfo() {
            Console.WriteLine("GetSecurityInfo for RIM5: "
            + String.Join(",", _q.Class.GetSecurityInfo("SPBFUT", "RIM5").Result.ToJson()));

            Console.WriteLine("GetSecurityInfo for LKOH: "
            + String.Join(",", _q.Class.GetSecurityInfo("TQBR", "LKOH").Result.ToJson()));
        }

    }
}
