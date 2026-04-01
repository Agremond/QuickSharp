// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Таблица с параметрами обезличенной сделки
    /// </summary>
    public class AllTrade : IWithLuaTimeStamp
    {
        /// <summary>
        /// Номер сделки в торговой системе
        /// </summary>
        [JsonPropertyName("trade_num")]
        public double TradeNum { get; set; }

        /// <summary>
        /// Набор битовых флагов:
        /// бит 0 (0x1)  Сделка на продажу
        /// бит 1 (0x2)  Сделка на покупку
        /// </summary>
        [JsonPropertyName("flags")]
        public AllTradeFlags Flags { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        [JsonPropertyName("price")]
        public double Price { get; set; }

        /// <summary>
        /// Количество бумаг в последней сделке в лотах
        /// </summary>
        [JsonPropertyName("qty")]
        public double Qty { get; set; }

        /// <summary>
        /// Объем в денежных средствах
        /// </summary>
        [JsonPropertyName("value")]
        public double Value { get; set; }

        /// <summary>
        /// Накопленный купонный доход
        /// </summary>
        [JsonPropertyName("accruedint")]
        public double Accruedint { get; set; }

        /// <summary>
        /// Доходность
        /// </summary>
        [JsonPropertyName("yield")]
        public double Yield { get; set; }

        /// <summary>
        /// Код расчетов
        /// </summary>
        [JsonPropertyName("settlecode")]
        public string Settlecode { get; set; }

        /// <summary>
        /// Ставка РЕПО (%)
        /// </summary>
        [JsonPropertyName("reporate")]
        public double Reporate { get; set; }

        /// <summary>
        /// Сумма РЕПО
        /// </summary>
        [JsonPropertyName("repovalue")]
        public double Repovalue { get; set; }

        /// <summary>
        /// Объем выкупа РЕПО
        /// </summary>
        [JsonPropertyName("repo2value")]
        public double Repo2Value { get; set; }

        /// <summary>
        /// Срок РЕПО в днях
        /// </summary>
        [JsonPropertyName("repoterm")]
        public double Repoterm { get; set; }

        /// <summary>
        /// Код бумаги заявки
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Дата и время
        /// </summary>
        [JsonPropertyName("datetime")]
        public QuikDateTime Datetime { get; set; }

        /// <summary>
        /// Период торговой сессии. Возможные значения:
        /// «0» – Открытие;
        /// «1» – Нормальный;
        /// «2» – Закрытие
        /// </summary>
        [JsonPropertyName("period")]
        public int Period { get; set; }

        /// <summary>
        /// Открытый интерес
        /// </summary>
        [JsonPropertyName("open_interest")]
        public double OpenInterest { get; set; }

        /// <summary>
        /// Код биржи в торговой системе
        /// </summary>
        [JsonPropertyName("exchange_code")]
        public string ExchangeCode { get; set; }

        /// <summary>
        /// Площадка исполнения
        /// </summary>
        [JsonPropertyName("exec_market")]
        public string ExecMarket { get; set; }

        /// <summary>
        /// Идентификатор индикативной ставки
        /// </summary>
        [JsonPropertyName("benchmark")]
        public string Benchmark { get; set; }

        public double LuaTimeStamp { get; set; }
    }
}