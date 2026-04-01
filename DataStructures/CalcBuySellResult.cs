using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    public class CalcBuySellResult
    {
        /// <summary>
        /// Максимально возможное количество бумаги
        /// </summary>
        [JsonPropertyName("qty")]
        public int Qty { get; set; }

        /// <summary>
        /// Сумма комиссии
        /// </summary>
        [JsonPropertyName("comission")]
        public double Comission { get; set; }
    }
}