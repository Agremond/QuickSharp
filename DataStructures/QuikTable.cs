// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Таблицы, используемые в функциях «getItem», «getOrderByNumber», «getNumberOf» и «SearchItems»
    /// </summary>
    internal class QuikTable
    {
        /// <summary>
        /// Фирмы
        /// </summary>
        [JsonPropertyName("firms")]
        public string Firms { get; set; }

        /// <summary>
        /// Классы
        /// </summary>
        [JsonPropertyName("classes")]
        public string Classes { get; set; }

        /// <summary>
        /// Инструменты
        /// </summary>
        [JsonPropertyName("securities")]
        public string Securities { get; set; }

        /// <summary>
        /// Торговые счета
        /// </summary>
        [JsonPropertyName("trade_accounts")]
        public string TradeAccounts { get; set; }

        /// <summary>
        /// Коды клиентов
        ///  - функция getNumberOf("client_codes") возвращает количество доступных кодов клиента в терминале, а функция getItem("client_codes", i) - строку содержащую клиентский код с индексом i, где i может принимать значения от 0 до getNumberOf("client_codes") -1
        /// </summary>
        [JsonPropertyName("client_codes")]
        public string ClientCodes { get; set; }

        /// <summary>
        /// Обезличенные сделки
        /// </summary>
        [JsonPropertyName("all_trades")]
        public string AllTrades { get; set; }

        /// <summary>
        /// Денежные позиции
        /// </summary>
        [JsonPropertyName("account_positions")]
        public string AccountPositions { get; set; }

        /// <summary>
        /// Заявки
        /// </summary>
        [JsonPropertyName("orders")]
        public string Orders { get; set; }

        /// <summary>
        /// Позиции по клиентским счетам (фьючерсы)
        /// </summary>
        [JsonPropertyName("futures_client_holding")]
        public string FuturesClientHolding { get; set; }

        /// <summary>
        /// Лимиты по фьючерсам
        /// </summary>
        [JsonPropertyName("futures_client_limits")]
        public string FuturesClientLimits { get; set; }

        /// <summary>
        /// Лимиты по денежным средствам
        /// </summary>
        [JsonPropertyName("money_limits")]
        public string MoneyLimits { get; set; }

        /// <summary>
        /// Лимиты по бумагам
        /// </summary>
        [JsonPropertyName("depo_limits")]
        public string DepoLimits { get; set; }

        /// <summary>
        /// Сделки
        /// </summary>
        [JsonPropertyName("trades")]
        public string Trades { get; set; }

        /// <summary>
        /// Стоп-заявки
        /// </summary>
        [JsonPropertyName("stop_orders")]
        public string StopOrders { get; set; }

        /// <summary>
        /// Заявки на внебиржевые сделки
        /// </summary>
        [JsonPropertyName("neg_deals")]
        public string NegDeals { get; set; }

        /// <summary>
        /// Сделки для исполнения
        /// </summary>
        [JsonPropertyName("neg_trades")]
        public string NegTrades { get; set; }

        /// <summary>
        /// Отчеты по сделкам для исполнения
        /// </summary>
        [JsonPropertyName("neg_deal_reports")]
        public string NegDealReports { get; set; }

        /// <summary>
        /// Позиции участника по инструментам
        /// </summary>
        [JsonPropertyName("firm_holding")]
        public string FirmHolding { get; set; }

        /// <summary>
        /// Текущие позиции клиентским счетам
        /// </summary>
        [JsonPropertyName("account_balance")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// Обязательства и требования по активам
        /// </summary>
        [JsonPropertyName("ccp_holdings")]
        public string CCPHoldings { get; set; }

        /// <summary>
        /// Валюта: обязательства и требования по активам
        /// </summary>
        [JsonPropertyName("rm_holdings")]
        public string RMHoldings { get; set; }

        /// <summary>
        /// Обязательства и требования по деньгам
        /// </summary>
        [JsonPropertyName("ccp_positions")]
        public string CCPPositions { get; set; }
    }
}