// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// При удалении клиентского лимита по бумагам функция возвращает таблицу Lua с параметрами
    /// </summary>
    public class MoneyLimitDelete
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
        /// Код клиента
        /// </summary>
        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Тип лимита. Возможные значения:
        /// «0» – обычные лимиты,
        /// иначе – технологические лимиты
        /// </summary>
        [JsonPropertyName("limit_kind")]
        public int LimitKind { get; set; }

        // ReSharper restore InconsistentNaming
    }
}