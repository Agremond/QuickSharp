// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// При обработке удаления бумажного лимита функция возвращает таблицу Lua "Удаление позиции по инструментам" с параметрами
    /// </summary>
    public class DepoLimitDelete
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Код инструмента
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код торгового счета
        /// </summary>
        [JsonPropertyName("trdaccid")]
        public string TrdAccId { get; set; }

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
        /// Тип лимита. Возможные значения:
        /// ///«0» – обычные лимиты,
        /// ///значение не равное «0» – технологические лимиты
        /// </summary>
        [JsonPropertyName("limit_kind")]
        public int LimitKindInt { get; set; }

        // ReSharper restore InconsistentNaming
    }
}