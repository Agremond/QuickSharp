using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров Таблицы обязательств и требований по активам на валютном рынке
    /// </summary>
    public class RMHolding
    {
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
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

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
        /// Дата расчётов
        /// </summary>
        [JsonPropertyName("date")]
        public int Date { get; set; }

        /// <summary>
        /// Дебит (Размер денежных обязательств)
        /// </summary>
        [JsonPropertyName("debit")]
        public double Debit { get; set; }

        /// <summary>
        /// Кредит (Размер денежных требований)
        /// </summary>
        [JsonPropertyName("credit")]
        public double Credit { get; set; }

        /// <summary>
        /// Сумма денежных средств в заявках на покупку
        /// </summary>
        [JsonPropertyName("value_buy")]
        public double ValueBuy{ get; set; }

        /// <summary>
        /// Сумма денежных средств в заявках на продажу
        /// </summary>
        [JsonPropertyName("value_sell")]
        public double ValueSell { get; set; }

        /// <summary>
        /// Сумма возврата компенсационного перевода
        /// </summary>
        [JsonPropertyName("margin_call")]
        public double MarginCall { get; set; }

        /// <summary>
        /// Плановая позиция Т+
        /// </summary>
        [JsonPropertyName("planned_covered")]
        public long PlannedCovered { get; set; }

        /// <summary>
        /// Размер денежных обязательств на начало дня, с точностью до 2 знаков после десятичного разделителя
        /// </summary>
        [JsonPropertyName("debit_balance")]
        public double DebitBalance { get; set; }

        /// <summary>
        /// Размер денежных требований на начало дня, с точностью до 2 знаков после десятичного разделителя
        /// </summary>
        [JsonPropertyName("credit_balance")]
        public double CreditBalance { get; set; }
    }
}