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
        /// Дата экспирации
        /// </summary>
        [JsonPropertyName("MAT_DATE")]
        public string ExpDate { get; set; }

        /// <summary>
        /// DaysToMatDate
        /// </summary>
        [JsonPropertyName("DAYS_TO_MAT_DATE")]
        public double DAYSTOMATDATE { get; set; }
        /// <summary>
        /// LastPrice
        /// </summary>
        [JsonPropertyName("Lastprice")]
        public double LastPrice { get; set; }

        /// <summary>
        /// TheorPrice
        /// </summary>
        [JsonPropertyName("THEORPRICE")]
        public double TheorPrice { get; set; }

        /// <summary>
        /// Шаг цены
        /// </summary>
        [JsonPropertyName("SEC_PRICE_STEP")]
        public double Step { get; set; }

        /// <summary>
        /// Стоимость шага цены
        /// </summary>
        [JsonPropertyName("STEPPRICET")]
        public double StepPrice { get; set; }
        /// <summary>
        /// Размер лота
        /// </summary>
        [JsonPropertyName("LOTSIZE")]
        public int Lot { get; set; }

        /// <summary>
        /// Гарантийное обеспечение покуптеля
        /// </summary>
        [JsonPropertyName("BUYDEPO")]
        public double BuyDepo { get; set; }


        /// <summary>
        /// Гарантийное обеспечение продавца
        /// </summary>
        [JsonPropertyName("SELLDEPO")]
        public double SellDepo { get; set; }
    }
}