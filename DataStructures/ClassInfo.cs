// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание класса
    /// </summary>
    public class ClassInfo
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Код фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Наименование класса
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Количество параметров в классе
        /// </summary>
        [JsonPropertyName("npars")]
        public int NPars { get; set; }

        /// <summary>
        /// Количество бумаг в классе
        /// </summary>
        [JsonPropertyName("nsecs")]
        public int NSecs { get; set; }

        // ReSharper restore InconsistentNaming

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}