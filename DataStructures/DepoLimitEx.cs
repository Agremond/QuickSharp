using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Лимиты по бумагам (таблица depo_limits)
    /// </summary>
    public class DepoLimitEx
    {
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; } = string.Empty;

        [JsonPropertyName("trdaccid")]
        public string TrdAccId { get; set; } = string.Empty;

        [JsonPropertyName("firmid")]
        public string FirmId { get; set; } = string.Empty;

        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; } = string.Empty;

        [JsonPropertyName("balaccid")]
        public string BalAccId { get; set; } = string.Empty;

        [JsonPropertyName("openbal")]
        public double OpenBalance { get; set; }

        [JsonPropertyName("openlimit")]
        public double OpenLimit { get; set; }

        [JsonPropertyName("currentbal")]
        public double CurrentBalance { get; set; }

        [JsonPropertyName("currentlimit")]
        public double CurrentLimit { get; set; }

        [JsonPropertyName("locked_sell")]
        public double LockedSell { get; set; }

        [JsonPropertyName("locked_buy")]
        public double LockedBuy { get; set; }

        [JsonPropertyName("locked_buy_value")]
        public double LockedBuyValue { get; set; }

        [JsonPropertyName("locked_sell_value")]
        public double LockedSellValue { get; set; }

        [JsonPropertyName("awg_position_price")]
        public double AvgPositionPrice { get; set; }        // было AweragePositionPrice

        // Дополнительное поле из QUIK (часто приходит вместо/вместе с awg)
        [JsonPropertyName("wa_position_price")]
        public double WaPositionPrice { get; set; }

        [JsonPropertyName("wa_price_currency")]
        public string WaPriceCurrency { get; set; } = string.Empty;

        [JsonPropertyName("limit_kind")]
        public int LimitKindInt { get; set; }

        [JsonIgnore]
        public LimitKind LimitKind { get; private set; }

        // Автоматическое преобразование limit_kind
        [JsonPropertyName("limit_kind_name")]
        public string LimitKindName { get; set; } = string.Empty;

        // Вызывается после десериализации
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        public DepoLimitEx()
        {
            // Конструктор по умолчанию
        }

        // Этот метод можно вызвать вручную после десериализации, если нужно
        public void UpdateLimitKind()
        {
            LimitKind = LimitKindInt switch
            {
                0 => LimitKind.T2,   // в QUIK обычно 0 = T+2
                1 => LimitKind.T1,
                2 => LimitKind.T0,
                _ => LimitKind.NotImplemented
            };
        }
    }

    public enum LimitKind
    {
        T0,
        T1,
        T2,
        NotImplemented
    }
}