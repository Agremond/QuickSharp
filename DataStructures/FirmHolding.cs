using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров таблицы «Позиции участника по инструментам»
    /// </summary>
    public class FirmHolding
    {
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Код инструмента
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Входящий остаток
        /// </summary>
        [JsonPropertyName("openbal")]
        public double OpenBal { get; set; }

        /// <summary>
        /// Текущий остаток
        /// </summary>
        [JsonPropertyName("currentpos")]
        public double CurrentPos { get; set; }

        /// <summary>
        /// Количество инструментов в активных заявках на покупку 
        /// </summary>
        [JsonPropertyName("plannedposbuy")]
        public double PlannedPosBuy { get; set; }

        /// <summary>
        /// Количество инструментов в активных заявках на продажу
        /// </summary>
        [JsonPropertyName("plannedpossell")]
        public double PlannedPosSell { get; set; }

        /// <summary>
        /// Куплено
        /// </summary>
        [JsonPropertyName("usqtyb")]
        public double UsQtyB { get; set; }

        /// <summary>
        /// Продано
        /// </summary>
        [JsonPropertyName("usqtys")]
        public double UsQtyS { get; set; }
    }
}