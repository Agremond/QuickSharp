using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров Таблицы обязательств и требований по активам
    /// </summary>
    public class CCPHolding
    {
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Номер счета депо в Депозитарии (НДЦ)
        /// </summary>
        [JsonPropertyName("depo_account")]
        public string DepoAccount { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Идентификатор расчетного счета/кода в клиринговой организации
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccId { get; set; }

        /// <summary>
        /// Дата расчетов
        /// </summary>
        [JsonPropertyName("settle_date")]
        public int SettleDate { get; set; }

        /// <summary>
        /// Количество инструментов в сделках
        /// </summary>
        [JsonPropertyName("qty")]
        public long Quantity { get; set; }

        /// <summary>
        /// Количество инструментов в заявках на покупку
        /// </summary>
        [JsonPropertyName("qty_buy")]
        public long QuantityBuy { get; set; }

        /// <summary>
        /// Количество инструментов в заявках на продажу
        /// </summary>
        [JsonPropertyName("qty_sell")]
        public long QuantitySell { get; set; }

        /// <summary>
        /// Нетто-позиция
        /// </summary>
        [JsonPropertyName("netto")]
        public long Netto { get; set; }

        /// <summary>
        /// Дебит
        /// </summary>
        [JsonPropertyName("debit")]
        public double Debit { get; set; }

        /// <summary>
        /// Кредит
        /// </summary>
        [JsonPropertyName("credit")]
        public double Credit { get; set; }

        /// <summary>
        /// Код инструмента
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Плановая позиция Т+
        /// </summary>
        [JsonPropertyName("planned_covered")]
        public long PlannedCovered { get; set; }

        /// <summary>
        /// Тип раздела. Возможные значения: 
        /// «0» – торговый раздел; 
        /// «1» – раздел обеспечения
        /// </summary>
        [JsonPropertyName("firm_use")]
        public int FirmUse { get; set; }
    }
}