
using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures.Transaction
{
    /// <summary>
    /// Описание параметров таблицы Отчеты по сделкам для исполнения
    /// </summary>
    public class NegDealReport : IWithLuaTimeStamp
    {
        [JsonPropertyName("lua_timestamp")]
        public double LuaTimeStamp { get; internal set; }

        /// <summary>
        /// Отчет
        /// </summary>
        [JsonPropertyName("report_num")]
        public long ReportNumber { get; set; }

        /// <summary>
        /// Дата отчета
        /// </summary>
        [JsonPropertyName("report_date")]
        public int ReportDate { get; set; }

        /// <summary>
        /// Набор битовых флагов
        /// </summary>
        [JsonPropertyName("flags")]
        public NegReportFlags Flags { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Счет депо
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Код фирмы партнера
        /// </summary>
        [JsonPropertyName("cpfirmid")]
        public string CpFirmId { get; set; }

        /// <summary>
        /// Код торгового счета партнера
        /// </summary>
        [JsonPropertyName("cpaccount")]
        public string CpAccount { get; set; }

        /// <summary>
        /// Количество инструментов, в лотах
        /// </summary>
        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        /// <summary>
        /// Объем сделки, выраженный в рублях
        /// </summary>
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Время снятия заявки
        /// </summary>
        [JsonPropertyName("withdraw_time")]
        public int WithdrawTime { get; set; }

        /// <summary>
        /// Тип отчета
        /// </summary>
        [JsonPropertyName("report_type")]
        public int ReportType { get; set; }

        /// <summary>
        /// Вид отчета
        /// </summary>
        [JsonPropertyName("report_kind")]
        public int ReportKind { get; set; }

        /// <summary>
        /// Объем комиссии по сделке, выраженный в руб
        /// </summary>
        [JsonPropertyName("commission")]
        public decimal Commission { get; set; }

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
        /// Время отчета
        /// </summary>
        [JsonPropertyName("report_time")]
        public int ReportTime { get; set; }

        /// <summary>
        /// Дата и время отчета
        /// </summary>
        [JsonPropertyName("report_date_time")]
        public QuikDateTime ReportDateTime { get; set; }
    }
}