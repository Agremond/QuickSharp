// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров таблицы Торговые счета
    /// </summary>
    public class TradesAccounts
    {
        /// <summary>
        /// Список кодов классов, разделенных символом «|»
        /// </summary>
        [JsonPropertyName("class_codes")]
        public string ClassCodes { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string Firmid { get; set; }

        /// <summary>
        /// Код торгового счета
        /// </summary>
        [JsonPropertyName("trdaccid")]
        public string TrdaccId { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Запрет необеспеченных продаж. Возможные значения:
        /// «0» – Нет;
        /// «1» – Да
        /// </summary>
        [JsonPropertyName("fullcoveredsell")]
        public int Fullcoveredsell { get; set; }

        /// <summary>
        /// Номер основного торгового счета
        /// </summary>
        [JsonPropertyName("main_trdaccid")]
        public string MainTrdaccid { get; set; }

        /// <summary>
        /// Расчетная организация по «Т0»
        /// </summary>
        [JsonPropertyName("bankid_t0")]
        public string BankIdT0 { get; set; }

        /// <summary>
        /// Расчетная организация по «Т+»
        /// </summary>
        [JsonPropertyName("bankid_tplus")]
        public string BankidTplus { get; set; }

        /// <summary>
        /// Тип депозитарного счета
        /// </summary>
        [JsonPropertyName("trdacc_type")]
        public int TrdaccType { get; set; }

        /// <summary>
        /// Раздел счета Депо
        /// </summary>
        [JsonPropertyName("depunitid")]
        public string DepunitId { get; set; }

        /// <summary>
        /// Статус торгового счета. Возможные значения:
        /// «0» – операции разрешены;
        /// «1» – операции запрещены
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// Тип раздела. Возможные значения:
        /// «0» – раздел обеспечения;
        /// иначе – для торговых разделов
        /// </summary>
        [JsonPropertyName("firmuse")]
        public int Firmuse { get; set; }

        /// <summary>
        /// Номер счета депо в депозитарии
        /// </summary>
        [JsonPropertyName("depaccid")]
        public string DepaccId { get; set; }

        /// <summary>
        /// Код дополнительной позиции по денежным средствам
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccId { get; set; }
    }
}