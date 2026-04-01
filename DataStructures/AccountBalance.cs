// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// При изменении текущей позиции по счету функция возвращает таблицу Lua «Позиции участника по торговым счетам» с параметрами
    /// </summary>
    public class AccountBalance
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Код бумаги
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("trdaccid")]
        public string TrdAccId { get; set; }

        /// <summary>
        /// Счет депо
        /// </summary>
        [JsonPropertyName("depaccid")]
        public string DepAccId { get; set; }

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
        /// Плановая продажа
        /// </summary>
        [JsonPropertyName("plannedpossell")]
        public double PlannedPosSell { get; set; }

        /// <summary>
        /// Плановая покупка
        /// </summary>
        [JsonPropertyName("plannedposbuy")]
        public double PlannedPosBuy { get; set; }

        /// <summary>
        /// Контрольный остаток простого клиринга, равен входящему остатку минус плановая позиция на продажу, включенная в простой клиринг
        /// </summary>
        [JsonPropertyName("planbal")]
        public double PlanBal { get; set; }

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

        /// <summary>
        /// Плановый остаток, равен текущему остатку минус плановая позиция на продажу
        /// </summary>
        [JsonPropertyName("planned")]
        public double Planned { get; set; }

        /// <summary>
        /// Плановая позиция после проведения расчетов
        /// </summary>
        [JsonPropertyName("settlebal")]
        public double SettleBal { get; set; }

        /// <summary>
        /// Идентификатор расчетного счета/кода в клиринговой организации
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccId { get; set; }

        /// <summary>
        /// Признак счета обеспечения. Возможные значения:
        /// «0» – для обычных счетов,
        /// «1» – для счета обеспечения.
        /// </summary>
        [JsonPropertyName("firmuse")]
        public double FirmUse { get; set; }

        // ReSharper restore InconsistentNaming
    }
}