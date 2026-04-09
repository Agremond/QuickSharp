// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp
{
    /// <summary>
    ///
    /// </summary>
    public interface ISecurity
    {
        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("class_code")]
        string ClassCode { get; set; }

        /// <summary>
        ///
        /// </summary>
        [JsonPropertyName("sec_code")]
        string SecCode { get; set; }

        /// <summary>
        /// Свойство возвращает коммбинацию ClassCode и SecCode в выбраном пользователем формате,
        ///  например: $"{ClassCode}@{SecCode}" или $"{ClassCode}-{SecCode}"
        /// </summary>
        string FullCode { get; }
    }
}