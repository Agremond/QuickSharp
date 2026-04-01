// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров таблицы «Позиции участника по деньгам»
    /// При изменении денежной позиции по счету функция возвращает таблицу Lua с параметрами
    /// </summary>
    public class AccountPosition
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

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
        /// Описание
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

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
        /// Плановый остаток
        /// </summary>
        [JsonPropertyName("plannedpos")]
        public double PlannedPos { get; set; }

        /// <summary>
        /// Внешнее ограничение по деньгам
        /// </summary>
        [JsonPropertyName("limit1")]
        public double Limit1 { get; set; }

        /// <summary>
        /// Внутреннее (собственное) ограничение по деньгам
        /// </summary>
        [JsonPropertyName("limit2")]
        public double Limit2 { get; set; }

        /// <summary>
        /// В заявках на продажу // Странно. Не ошибка ли????
        /// </summary>
        [JsonPropertyName("orderbuy")]
        public double OrderBuy { get; set; }

        /// <summary>
        /// В заявках на покупку // Странно. Не ошибка ли????
        /// </summary>
        [JsonPropertyName("ordersell")]
        public double OrderSell { get; set; }

        /// <summary>
        /// Нетто-позиция
        /// </summary>
        [JsonPropertyName("netto")]
        public double Netto { get; set; }

        /// <summary>
        /// Плановая позиция
        /// </summary>
        [JsonPropertyName("plannedbal")]
        public double PlannedBal { get; set; }

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
        /// Идентификатор счета
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccId { get; set; }

        /// <summary>
        /// Маржинальное требование на начало торгов
        /// </summary>
        [JsonPropertyName("margincall")]
        public double MarginCall { get; set; }

        /// <summary>
        /// Плановая позиция после проведения расчетов
        /// </summary>
        [JsonPropertyName("settlebal")]
        public double SettleBal { get; set; }

        // ReSharper restore InconsistentNaming
    }
}