// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Позиции по деньгам (ранее - Лимиты по денежным средствам)
    /// </summary>
    public class MoneyLimitEx
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Код валюты
        /// </summary>
        [JsonPropertyName("currcode")]
        public string CurrCode { get; set; }

        /// <summary>
        /// Тэг расчетов
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Код клиента
        /// </summary>
        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Входящий остаток по деньгам
        /// </summary>
        [JsonPropertyName("openbal")]
        public double OpenBal { get; set; }

        /// <summary>
        /// Входящий лимит по деньгам
        /// </summary>
        [JsonPropertyName("openlimit")]
        public double OpenLimit { get; set; }

        /// <summary>
        /// Текущий остаток по деньгам
        /// </summary>
        [JsonPropertyName("currentbal")]
        public double CurrentBal { get; set; }

        /// <summary>
        /// Текущий лимит по деньгам
        /// </summary>
        [JsonPropertyName("currentlimit")]
        public double CurrentLimit { get; set; }

        /// <summary>
        /// Заблокированное количество
        /// </summary>
        [JsonPropertyName("locked")]
        public double Locked { get; set; }

        /// <summary>
        /// Стоимость активов в заявках на покупку немаржинальных бумаг
        /// </summary>
        [JsonPropertyName("locked_value_coef")]
        public double LockedValueCoef { get; set; }

        /// <summary>
        /// Стоимость активов в заявках на покупку маржинальных бумаг
        /// </summary>
        [JsonPropertyName("locked_margin_value")]
        public double LockedMarginValue { get; set; }

        /// <summary>
        /// Плечо
        /// </summary>
        [JsonPropertyName("leverage")]
        public double Leverage { get; set; }

        /// <summary>
        /// Тип лимита. Возможные значения:
        /// «0» – обычные лимиты,
        /// иначе – технологические лимиты
        /// </summary>
        [JsonPropertyName("limit_kind")]
        public int LimitKind { get; set; }

        /// <summary>
        /// Средневзвешенная цена приобретения позиции
        /// </summary>
        [JsonPropertyName("wa_position_price")]
        public double WaPositionPrice { get; set; }

        /// <summary>
        /// Гарантийное обеспечение заявок
        /// </summary>
        [JsonPropertyName("orders_collateral")]
        public double OrdersCollateral { get; set; }

        /// <summary>
        /// Гарантийное обеспечение позиций
        /// </summary>
        [JsonPropertyName("positions_collateral")]
        public double PositionsCollateral { get; set; }

        // ReSharper restore InconsistentNaming
    }
}