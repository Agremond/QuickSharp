using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Функции для работы со стоп-заявками через транспорт QUIK (SHM / TCP)
    /// </summary>
    public class StopOrderFunctions
    {
        private readonly IQuikTransport _transport;
        private readonly ITradingFunctions _trading;  // для отправки транзакций

        public delegate void StopOrderHandler(StopOrder stopOrder);
        public event StopOrderHandler? NewStopOrder;

        internal void RaiseNewStopOrderEvent(StopOrder stopOrder)
        {
            NewStopOrder?.Invoke(stopOrder);
        }

        public StopOrderFunctions(IQuikTransport transport, ITradingFunctions trading)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _trading = trading ?? throw new ArgumentNullException(nameof(trading));
        }

        /// <summary>
        /// Возвращает список всех стоп-заявок.
        /// </summary>
        public async Task<List<StopOrder>> GetStopOrders()
        {
            var response = await _transport
                .SendAsync<Message<string>, Message<List<StopOrder>>>(
                    new Message<string>("", "get_stop_orders"),
                    "get_stop_orders")
                .ConfigureAwait(false);

            return response.Data ?? new List<StopOrder>();
        }

        /// <summary>
        /// Возвращает список стоп-заявок для заданного инструмента.
        /// </summary>
        public async Task<List<StopOrder>> GetStopOrders(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            var response = await _transport
                .SendAsync<Message<string>, Message<List<StopOrder>>>(
                    new Message<string>(payload, "get_stop_orders"),
                    "get_stop_orders")
                .ConfigureAwait(false);

            return response.Data ?? new List<StopOrder>();
        }

        /// <summary>
        /// Создаёт новую стоп-заявку.
        /// </summary>
        public async Task<long> CreateStopOrder(StopOrder stopOrder)
        {
            if (stopOrder == null) throw new ArgumentNullException(nameof(stopOrder));

            var trans = new Transaction
            {
                ACTION = TransactionAction.NEW_STOP_ORDER,
                ACCOUNT = stopOrder.Account,
                CLASSCODE = stopOrder.ClassCode,
                SECCODE = stopOrder.SecCode,
                EXPIRY_DATE = "GTC", // до отмены
                STOPPRICE = stopOrder.ConditionPrice,
                PRICE = stopOrder.Price,
                QUANTITY = stopOrder.Quantity,
                STOP_ORDER_KIND = ConvertStopOrderType(stopOrder.StopOrderType),
                OPERATION = stopOrder.Operation == Operation.Buy ? TransactionOperation.B : TransactionOperation.S
            };

            // Дополнительно для Take-Profit и Take-Profit + Stop-Limit
            if (stopOrder.StopOrderType is StopOrderType.TakeProfit or StopOrderType.TakeProfitStopLimit)
            {
                trans.OFFSET = stopOrder.Offset;
                trans.SPREAD = stopOrder.Spread;
                trans.OFFSET_UNITS = stopOrder.OffsetUnit;
                trans.SPREAD_UNITS = stopOrder.SpreadUnit;
            }

            if (stopOrder.StopOrderType == StopOrderType.TakeProfitStopLimit)
                trans.STOPPRICE2 = stopOrder.ConditionPrice2;

            return await _trading.SendTransaction(trans).ConfigureAwait(false);
        }

        /// <summary>
        /// Отменяет существующую стоп-заявку.
        /// </summary>
        public async Task<long> KillStopOrder(StopOrder stopOrder)
        {
            if (stopOrder == null) throw new ArgumentNullException(nameof(stopOrder));
            if (stopOrder.OrderNum <= 0)
                throw new ArgumentException("У стоп-заявки должен быть заполнен OrderNum", nameof(stopOrder));

            var trans = new Transaction
            {
                ACTION = TransactionAction.KILL_STOP_ORDER,
                CLASSCODE = stopOrder.ClassCode,
                SECCODE = stopOrder.SecCode,
                STOP_ORDER_KEY = stopOrder.OrderNum.ToString()
            };

            return await _trading.SendTransaction(trans).ConfigureAwait(false);
        }

        private static StopOrderKind ConvertStopOrderType(StopOrderType stopOrderType)
        {
            return stopOrderType switch
            {
                StopOrderType.StopLimit => StopOrderKind.SIMPLE_STOP_ORDER,
                StopOrderType.TakeProfit => StopOrderKind.TAKE_PROFIT_STOP_ORDER,
                StopOrderType.TakeProfitStopLimit => StopOrderKind.TAKE_PROFIT_AND_STOP_LIMIT_ORDER,
                _ => throw new ArgumentException($"Не поддерживаемый тип стоп-заявки: {stopOrderType}")
            };
        }
    }
}