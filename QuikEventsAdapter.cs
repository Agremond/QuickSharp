using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;

namespace QuikSharp
{
    /// <summary>
    /// Адаптер событий транспорта в IQuikEvents
    /// </summary>
    public class QuikEventsAdapter : IQuikEvents
    {
        private readonly IQuikTransport _transport;

        public QuikEventsAdapter(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

            // Проброс всех событий транспорта в интерфейс IQuikEvents
            _transport.OnNewCandle += (c) => OnNewCandle?.Invoke(c);
            _transport.OnOrder += (o) => OnOrder?.Invoke(o);
            _transport.OnTrade += (t) => OnTrade?.Invoke(t);
            _transport.OnTransReply += (r) => OnTransReply?.Invoke(r);
            _transport.OnStopOrder += (s) => OnStopOrder?.Invoke(s);
            _transport.OnAllTrade += (a) => OnAllTrade?.Invoke(a);
            _transport.OnQuote += (q) => OnQuote?.Invoke(q);
            _transport.OnParam += (p) => OnParam?.Invoke(p);
            _transport.OnAccountBalance += (a) => OnAccountBalance?.Invoke(a);
            _transport.OnAccountPosition += (p) => OnAccountPosition?.Invoke(p);
            _transport.OnDepoLimit += (d) => OnDepoLimit?.Invoke(d);
            _transport.OnDepoLimitDelete += (d) => OnDepoLimitDelete?.Invoke(d);
            _transport.OnFirm += (f) => OnFirm?.Invoke(f);
            _transport.OnFuturesClientHolding += (f) => OnFuturesClientHolding?.Invoke(f);
            _transport.OnFuturesLimitChange += (f) => OnFuturesLimitChange?.Invoke(f);
            _transport.OnFuturesLimitDelete += (f) => OnFuturesLimitDelete?.Invoke(f);
            _transport.OnMoneyLimit += (m) => OnMoneyLimit?.Invoke(m);
            _transport.OnMoneyLimitDelete += (m) => OnMoneyLimitDelete?.Invoke(m);
            _transport.OnConnected += () => OnConnected?.Invoke();
            _transport.OnDisconnected += () => OnDisconnected?.Invoke();
        }

        // Проброс событий интерфейса IQuikEvents
        public event Action<Candle> OnNewCandle;
        public event Action<Order> OnOrder;
        public event Action<Trade> OnTrade;
        public event Action<TransactionReply> OnTransReply;
        public event Action<StopOrder> OnStopOrder;
        public event Action<AllTrade> OnAllTrade;
        public event Action<OrderBook> OnQuote;
        public event Action<Param> OnParam;
        public event Action<AccountBalance> OnAccountBalance;
        public event Action<AccountPosition> OnAccountPosition;
        public event Action<DepoLimitEx> OnDepoLimit;
        public event Action<DepoLimitDelete> OnDepoLimitDelete;
        public event Action<Firm> OnFirm;
        public event Action<FuturesClientHolding> OnFuturesClientHolding;
        public event Action<FuturesLimits> OnFuturesLimitChange;
        public event Action<FuturesLimitDelete> OnFuturesLimitDelete;
        public event Action<MoneyLimitEx> OnMoneyLimit;
        public event Action<MoneyLimitDelete> OnMoneyLimitDelete;
        public event Action OnConnected;
        public event Action OnDisconnected;
    }
}