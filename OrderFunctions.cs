using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Класс для работы с заявками через новый транспорт SHM.
    /// </summary>
    public class OrderFunctions
    {
        private readonly IQuikTransport _transport;

        public OrderFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }


        /// <summary>
        /// Создание новой заявки.
        /// </summary>
        /// <param name="order">Инфомация о новой заявки, на основе которой будет сформирована транзакция.</param>
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
                EXECUTION_CONDITION = order.ExecType == 1
                    ? ExecutionCondition.FILL_OR_KILL
                    : ExecutionCondition.PUT_IN_QUEUE
            };

            return await _transport.SendTransaction(txn).ConfigureAwait(false);
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
                ORDER_KEY = order.OrderNum.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };

            return await _transport.SendTransaction(txn).ConfigureAwait(false);
        }

        /// <summary>
        /// Получение заявки по номеру (classCode + orderId). Возвращает null, если заявка не найдена.
        /// </summary>
        public async Task<Order?> GetOrder(string classCode, long orderId, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{orderId}", "get_order_by_number");

            var orders = await _transport.SendAsync<Message, Order[]>(msg, "get_order_by_number", ct)
                                        .ConfigureAwait(false);

            return orders?.FirstOrDefault();
        }

        /// <summary>
        /// Получение заявки по номеру (только orderNum). Возвращает null, если заявка не найдена.
        /// </summary>
        public async Task<Order?> GetOrder_by_Number(long orderNum, CancellationToken ct = default)
        {
            var msg = new Message(orderNum.ToString(), "get_order_by_number");

            var orders = await _transport.SendAsync<Message, Order[]>(msg, "get_order_by_number", ct)
                                        .ConfigureAwait(false);

            return orders?.FirstOrDefault();
        }

        /// <summary>
        /// Получение заявки по ID транзакции. Возвращает null, если заявка не найдена.
        /// </summary>
        public async Task<Order?> GetOrder_by_transID(string classCode, string securityCode, long transId, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{securityCode}|{transId}", "getOrder_by_ID");

            var orders = await _transport.SendAsync<Message, Order[]>(msg, "getOrder_by_ID", ct)
                                        .ConfigureAwait(false);

            return orders?.FirstOrDefault();
        }

        /// <summary>
        /// Получение всех заявок.
        /// </summary>
        public async Task<List<Order>> GetOrders(CancellationToken ct = default)
        {
            var msg = new Message("", "get_orders");

            var orders = await _transport.SendAsync<Message, List<Order>>(msg, "get_orders", ct)
                                        .ConfigureAwait(false);

            return orders ?? new List<Order>();
        }

        /// <summary>
        /// Получение заявок по инструменту (classCode + secCode).
        /// </summary>
        public async Task<List<Order>> GetOrders(string classCode, string securityCode, CancellationToken ct = default)
        {
            var msg = new Message($"{classCode}|{securityCode}", "get_orders");

            var orders = await _transport.SendAsync<Message, List<Order>>(msg, "get_orders", ct)
                                        .ConfigureAwait(false);

            return orders ?? new List<Order>();
        }
    }
}