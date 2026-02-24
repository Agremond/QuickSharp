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

            // Проброс событий транспорта на делегаты интерфейса
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

        // ======== Реализация интерфейса IQuikEvents ========
        public event CandleHandler OnNewCandle;
        public event OrderHandler OnOrder;
        public event TradeHandler OnTrade;
        public event TransReplyHandler OnTransReply;
        public event StopOrderHandler OnStopOrder;
        public event AllTradeHandler OnAllTrade;
        public event QuoteHandler OnQuote;
        public event ParamHandler OnParam;
        public event AccountBalanceHandler OnAccountBalance;
        public event AccountPositionHandler OnAccountPosition;
        public event DepoLimitHandler OnDepoLimit;
        public event DepoLimitDeleteHandler OnDepoLimitDelete;
        public event FirmHandler OnFirm;
        public event FuturesClientHoldingHandler OnFuturesClientHolding;
        public event FuturesLimitHandler OnFuturesLimitChange;
        public event FuturesLimitDeleteHandler OnFuturesLimitDelete;
        public event MoneyLimitHandler OnMoneyLimit;
        public event MoneyLimitDeleteHandler OnMoneyLimitDelete;
        public event VoidHandler OnConnected;
        public event VoidHandler OnDisconnected;

        // Новые события интерфейса IQuikEvents
        public event InitHandler OnInit;
        public event VoidHandler OnCleanUp;
        public event VoidHandler OnClose;
        public event InitHandler OnConnectedToQuik;
        public event InitHandler OnDisconnectedFromQuik;
        public event NegDealHandler OnNegDeal;
        public event NegTradeHandler OnNegTrade;
        public event StopHandler OnStop;
    }
}