using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Класс для работы с заявками через новый транспорт SHM / IQuikTransport.
    /// </summary>
    public class OrderFunctions
    {
        private readonly IQuikTransport Transport;

        public OrderFunctions(IQuikTransport transport)
        {
            Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        /// <summary>
        /// Создание новой заявки.
        /// </summary>
        public async Task<long> CreateOrder(Order order, CancellationToken ct = default)
        {
            var txn = new Transaction
            {
                ACTION = TransactionAction.NEW_ORDER,
                ACCOUNT = order.Account,
                CLASSCODE = order.ClassCode,
                SECCODE = order.SecCode,
                QUANTITY = order.Quantity,
                OPERATION = order.Operation == Operation.Buy ? TransactionOperation.B : TransactionOperation.S,
                PRICE = order.Price,
                CLIENT_CODE = order.ClientCode,
                EXECUTION_CONDITION = order.ExecType == 1 ? ExecutionCondition.FILL_OR_KILL : ExecutionCondition.PUT_IN_QUEUE
            };

            var message = new Message(txn, "send_transaction");
            var resp = await Transport.SendAsync<Message, Message>(message, "send_transaction", ct).ConfigureAwait(false);

            return resp.GetData<long>();
        }

        /// <summary>
        /// Отмена заявки.
        /// </summary>
        public async Task<long> KillOrder(Order order, CancellationToken ct = default)
        {
            var txn = new Transaction
            {
                ACTION = TransactionAction.KILL_ORDER,
                CLASSCODE = order.ClassCode,
                SECCODE = order.SecCode,
                ORDER_KEY = order.OrderNum.ToString()
            };

            var message = new Message(txn, "send_transaction");
            var resp = await Transport.SendAsync<Message, Message>(message, "send_transaction", ct).ConfigureAwait(false);

            return resp.GetData<long>();
        }

        /// <summary>
        /// Получение заявки по номеру.
        /// </summary>
        public async Task<Order> GetOrder(string classCode, long orderId, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{orderId}", "get_order_by_number");
            var resp = await Transport.SendAsync<Message, Message>(msg, "get_order_by_number", ct).ConfigureAwait(false);

            return resp.GetData<Order>();
        }

        /// <summary>
        /// Получение всех заявок.
        /// </summary>
        public async Task<List<Order>> GetOrders(CancellationToken ct = default)
        {
            var msg = new Message("", "get_orders");
            var resp = await Transport.SendAsync<Message, Message>(msg, "get_orders", ct).ConfigureAwait(false);

            return resp.GetData<List<Order>>();
        }

        /// <summary>
        /// Получение заявок для инструмента.
        /// </summary>
        public async Task<List<Order>> GetOrders(string classCode, string securityCode, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{securityCode}", "get_orders");
            var resp = await Transport.SendAsync<Message, Message>(msg, "get_orders", ct).ConfigureAwait(false);

            return resp.GetData<List<Order>>();
        }

        /// <summary>
        /// Получение заявки по ID транзакции.
        /// </summary>
        public async Task<Order> GetOrder_by_transID(string classCode, string securityCode, long transId, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{securityCode}|{transId}", "get_order_by_transID");
            var resp = await Transport.SendAsync<Message, Message>(msg, "get_order_by_transID", ct).ConfigureAwait(false);

            return resp.GetData<Order>();
        }

        /// <summary>
        /// Получение заявки по номеру.
        /// </summary>
        public async Task<Order> GetOrder_by_Number(long orderNum, CancellationToken ct = default)
        {
            var msg = new Message($"{orderNum}", "get_order_by_number");
            var resp = await Transport.SendAsync<Message, Message>(msg, "get_order_by_number", ct).ConfigureAwait(false);

            return resp.GetData<Order>();
        }
    }
}