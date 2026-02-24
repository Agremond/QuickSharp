using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures.Transaction
{
    /// <summary>
    /// Формат .tri-файла с параметрами транзакций
    /// Адаптированный под QLua, System.Text.Json
    /// </summary>
    public class Transaction
    {
        // Events
        public event TransReplyHandler OnTransReply;
        public event OrderHandler OnOrder;
        public event StopOrderHandler OnStopOrder;
        public event TradeHandler OnTrade;

        internal void OnTransReplyCall(TransactionReply reply)
        {
            OnTransReply?.Invoke(reply);
            Trace.Assert(TransactionReply == null);
            TransactionReply = reply;
        }

        internal void OnOrderCall(Order order)
        {
            OnOrder?.Invoke(order);
            Orders ??= new List<Order>();
            Orders.Add(order);
        }

        internal void OnStopOrderCall(StopOrder stopOrder)
        {
            OnStopOrder?.Invoke(stopOrder);
            StopOrders ??= new List<StopOrder>();
            StopOrders.Add(stopOrder);
        }

        internal void OnTradeCall(Trade trade)
        {
            OnTrade?.Invoke(trade);
            Trades ??= new List<Trade>();
            Trades.Add(trade);
        }

        // Transaction reply
        public TransactionReply TransactionReply { get; set; }
        public List<Order> Orders { get; set; }
        public List<StopOrder> StopOrders { get; set; }
        public List<Trade> Trades { get; set; }

        public bool IsComepleted()
        {
            if (Orders == null || Orders.Count == 0) return false;
            var last = Orders[^1];
            return !last.Flags.HasFlag(OrderTradeFlags.Active)
                   && !last.Flags.HasFlag(OrderTradeFlags.Canceled);
        }

        public string ErrorMessage { get; set; }

        // Transaction specification properties
        public string CLASSCODE { get; set; }
        public string SECCODE { get; set; }
        public TransactionAction? ACTION { get; set; }
        public string FIRM_ID { get; set; }
        public string ACCOUNT { get; set; }
        public string CLIENT_CODE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<int>))]
        public int QUANTITY { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal>))]
        public decimal PRICE { get; set; }

        public TransactionOperation? OPERATION { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<long?>))]
        public long? TRANS_ID { get; set; }

        public TransactionType? TYPE { get; set; }
        public YesOrNo? MARKET_MAKER_ORDER { get; set; }
        public ExecutionCondition? EXECUTION_CONDITION { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? REPOVALUE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? START_DISCOUNT { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? LOWER_DISCOUNT { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? UPPER_DISCOUNT { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? STOPPRICE { get; set; }

        public StopOrderKind? STOP_ORDER_KIND { get; set; }
        public string STOPPRICE_CLASSCODE { get; set; }
        public string STOPPRICE_SECCODE { get; set; }
        public string STOPPRICE_CONDITION { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? LINKED_ORDER_PRICE { get; set; }

        public string EXPIRY_DATE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? STOPPRICE2 { get; set; }

        public YesOrNo? MARKET_STOP_LIMIT { get; set; }
        public YesOrNo? MARKET_TAKE_PROFIT { get; set; }
        public YesOrNo? IS_ACTIVE_IN_TIME { get; set; }

        [JsonConverter(typeof(HHMMSSDateTimeConverter))]
        public DateTime? ACTIVE_FROM_TIME { get; set; }

        [JsonConverter(typeof(HHMMSSDateTimeConverter))]
        public DateTime? ACTIVE_TO_TIME { get; set; }

        public string PARTNER { get; set; }
        public string ORDER_KEY { get; set; }
        public string STOP_ORDER_KEY { get; set; }
        public string SETTLE_CODE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? PRICE2 { get; set; }

        public string REPOTERM { get; set; }
        public string REPORATE { get; set; }
        public YesOrNo? BLOCK_SECURITIES { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? REFUNDRATE { get; set; }

        [JsonPropertyName("brokerref")]
        public string Comment { get; set; }

        public YesOrNo? LARGE_TRADE { get; set; }
        public string CURR_CODE { get; set; }
        public ForAccount? FOR_ACCOUNT { get; set; }
        public string SETTLE_DATE { get; set; }
        public YesOrNo? KILL_IF_LINKED_ORDER_PARTLY_FILLED { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? OFFSET { get; set; }

        public OffsetUnits? OFFSET_UNITS { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? SPREAD { get; set; }

        public OffsetUnits? SPREAD_UNITS { get; set; }
        public string BASE_ORDER_KEY { get; set; }
        public YesOrNo? USE_BASE_ORDER_BALANCE { get; set; }
        public YesOrNo? ACTIVATE_IF_BASE_ORDER_PARTLY_FILLED { get; set; }
        public string BASE_CONTRACT { get; set; }
        public string MODE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<long?>))]
        public long? FIRST_ORDER_NUMBER { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<int?>))]
        public int? FIRST_ORDER_NEW_QUANTITY { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? FIRST_ORDER_NEW_PRICE { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<long?>))]
        public long? SECOND_ORDER_NUMBER { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<int?>))]
        public int? SECOND_ORDER_NEW_QUANTITY { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<decimal?>))]
        public decimal? SECOND_ORDER_NEW_PRICE { get; set; }

        public YesOrNo? KILL_ACTIVE_ORDERS { get; set; }
        public string NEG_TRADE_OPERATION { get; set; }

        [JsonConverter(typeof(ToStringNumberConverter<long?>))]
        public long? NEG_TRADE_NUMBER { get; set; }

        public string VOLUMEMN { get; set; }
        public string VOLUMEPL { get; set; }
        public string KFL { get; set; }
        public string KGO { get; set; }
        public string USE_KGO { get; set; }
        public YesOrNo? CHECK_LIMITS { get; set; }
        public string MATCHREF { get; set; }
        public string CORRECTION { get; set; }

        [JsonIgnore]
        public bool IsManual { get; set; }
    }

    #region Custom Converters

    // Конвертер чисел <-> строка
    public class ToStringNumberConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return default!;
            try
            {
                var str = reader.GetString();
                if (string.IsNullOrEmpty(str)) return default!;
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch { return default!; }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }

    // Конвертер времени в формате HHMMSS
    public class HHMMSSDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string Format = "HHmmss";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s) || s.Length != 6) return null;
            if (int.TryParse(s.Substring(0, 2), out int h) &&
                int.TryParse(s.Substring(2, 2), out int m) &&
                int.TryParse(s.Substring(4, 2), out int s2))
            {
                return new DateTime(1, 1, 1, h, m, s2);
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(Format));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    #endregion
}