using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;

namespace QuikSharp
{
    public class QuikEventsAdapter : IQuikEvents
    {
        private readonly IQuikTransport _transport;

        public QuikEventsAdapter(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

            _transport.OnOrder += o => OnOrder?.Invoke(o);
            _transport.OnTrade += t => OnTrade?.Invoke(t);
            _transport.OnTransReply += r => OnTransReply?.Invoke(r);
            _transport.OnStopOrder += s => OnStopOrder?.Invoke(s);
            _transport.OnAllTrade += a => OnAllTrade?.Invoke(a);
            _transport.OnQuote += q => OnQuote?.Invoke(q);
            _transport.OnParam += p => OnParam?.Invoke(p);
            _transport.OnAccountBalance += a => OnAccountBalance?.Invoke(a);
            _transport.OnAccountPosition += p => OnAccountPosition?.Invoke(p);
            _transport.OnDepoLimit += d => OnDepoLimit?.Invoke(d);
            _transport.OnDepoLimitDelete += d => OnDepoLimitDelete?.Invoke(d);
            _transport.OnFirm += f => OnFirm?.Invoke(f);
            _transport.OnFuturesClientHolding += f => OnFuturesClientHolding?.Invoke(f);
            _transport.OnFuturesLimitChange += f => OnFuturesLimitChange?.Invoke(f);
            _transport.OnFuturesLimitDelete += f => OnFuturesLimitDelete?.Invoke(f);
            _transport.OnMoneyLimit += m => OnMoneyLimit?.Invoke(m);
            _transport.OnMoneyLimitDelete += m => OnMoneyLimitDelete?.Invoke(m);
            _transport.OnConnected += () => OnConnected?.Invoke();
            _transport.OnDisconnected += () => OnDisconnected?.Invoke();
        }

        public event InitHandler OnConnectedToQuik;
        public event VoidHandler OnDisconnectedFromQuik;

        public event AccountBalanceHandler OnAccountBalance;
        public event AccountPositionHandler OnAccountPosition;
        public event AllTradeHandler OnAllTrade;
        public event VoidHandler OnCleanUp;
        public event VoidHandler OnClose;
        public event VoidHandler OnConnected;
        public event DepoLimitHandler OnDepoLimit;
        public event DepoLimitDeleteHandler OnDepoLimitDelete;
        public event VoidHandler OnDisconnected;
        public event FirmHandler OnFirm;
        public event FuturesClientHoldingHandler OnFuturesClientHolding;
        public event FuturesLimitHandler OnFuturesLimitChange;
        public event FuturesLimitDeleteHandler OnFuturesLimitDelete;
        public event InitHandler OnInit;
        public event MoneyLimitHandler OnMoneyLimit;
        public event MoneyLimitDeleteHandler OnMoneyLimitDelete;

        // ВАЖНО — правильный тип
        public event EventHandler OnNegDeal;
        public event EventHandler OnNegTrade;

        public event OrderHandler OnOrder;
        public event ParamHandler OnParam;
        public event QuoteHandler OnQuote;
        public event StopHandler OnStop;
        public event StopOrderHandler OnStopOrder;
        public event TradeHandler OnTrade;
        public event TransReplyHandler OnTransReply;
    }
}