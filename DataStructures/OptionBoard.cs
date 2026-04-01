// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// OptionBoard structure
    /// </summary>
    public class OptionBoard
    {
        /// <summary>
        /// Strike
        /// </summary>
        [JsonPropertyName("Strike")]
        public double Strike { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Volatility
        /// </summary>
        [JsonPropertyName("Volatility")]
        public double Volatility { get; set; }

        /// <summary>
        /// OptionBase
        /// </summary>
        [JsonPropertyName("OPTIONBASE")]
        public string OPTIONBASE { get; set; }

        /// <summary>
        /// Offer
        /// </summary>
        [JsonPropertyName("OFFER")]
        public double OFFER { get; set; }

        /// <summary>
        /// Longname
        /// </summary>
        [JsonPropertyName("Longname")]
        public string Longname { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// OptionType
        /// </summary>
        [JsonPropertyName("OPTIONTYPE")]
        public string OPTIONTYPE { get; set; }

        /// <summary>
        /// ShortName
        /// </summary>
        [JsonPropertyName("shortname")]
        public string Shortname { get; set; }

        /// <summary>
        /// Bid
        /// </summary>
        [JsonPropertyName("BID")]
        public double BID { get; set; }

        /// <summary>
        /// DaysToMatDate
        /// </summary>
        [JsonPropertyName("DAYS_TO_MAT_DATE")]
        public int DAYSTOMATDATE { get; set; }
    }
}