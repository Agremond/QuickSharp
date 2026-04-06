// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров таблицы Инструменты
    /// Результат getSecurityInfo
    /// </summary>
    public class SecurityInfo
    {
        /// <summary>
        /// Код инструмента
        /// Устаревший параметр?
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код инструмента
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Наименование инструмента
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Краткое наименование
        /// </summary>
        [JsonPropertyName("short_name")]
        public string ShortName { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Наименование класса
        /// </summary>
        [JsonPropertyName("class_name")]
        public string ClassName { get; set; }

        /// <summary>
        /// Номинал
        /// </summary>
        [JsonPropertyName("face_value")]
        public double FaceValue { get; set; }

        /// <summary>
        /// Код валюты номинала
        /// </summary>
        [JsonPropertyName("face_unit")]
        public string FaceUnit { get; set; }

        /// <summary>
        /// Количество значащих цифр после запятой
        /// </summary>
        [JsonPropertyName("scale")]
        public int Scale { get; set; }

        /// <summary>
        /// Дата погашения (в QLUA это число, но на самом деле дата записанная как YYYYMMDD),
        /// поэтому здесь сохраняем просто как строку
        /// </summary>
        [JsonPropertyName("mat_date")]
        public double MatDate { get; set; }

        /// <summary>
        /// Размер лота
        /// </summary>
        [JsonPropertyName("lot_size")]
        public int LotSize { get; set; }

        /// <summary>
        /// ISIN-код
        /// </summary>
        [JsonPropertyName("isin_code")]
        public string IsinCode { get; set; }

        /// <summary>
        /// Минимальный шаг цены
        /// </summary>
        [JsonPropertyName("min_price_step")]
        public double MinPriceStep { get; set; }

        /// <summary>
        /// Bloomberg ID
        /// </summary>
        [JsonPropertyName("bsid")]
        public string BsId { get; set; }

        /// <summary>
        /// CUSIP
        /// </summary>
        [JsonPropertyName("cusip_code")]
        public string CUSIPCode_code { get; set; }

        /// <summary>
        /// StockCode
        /// </summary>
        [JsonPropertyName("stock_code")]
        public string StockCode { get; set; }

        /// <summary>
        /// Размер купона
        /// </summary>
        [JsonPropertyName("couponvalue")]
        public double CouponValue { get; set; }

        /// <summary>
        /// Код котируемой валюты в паре
        /// </summary>
        [JsonPropertyName("first_currcode")]
        public string FirstCurrCode { get; set; }

        /// <summary>
        /// Код базовой валюты в паре
        /// </summary>
        [JsonPropertyName("second_currcode")]
        public string SecondCurrCode { get; set; }

        /// <summary>
        /// Код класса базового актива
        /// </summary>
        [JsonPropertyName("base_active_classcode")]
        public string BaseActiveClassCode { get; set; }

        /// <summary>
        /// Базовый актив
        /// </summary>
        [JsonPropertyName("base_active_seccode")]
        public string BaseActiveSecCode { get; set; }

        /// <summary>
        /// Страйк опциона
        /// </summary>
        [JsonPropertyName("option_strike")]
        public double OptionStrike { get; set; }

        /// <summary>
        /// Кратность при вводе количества
        /// </summary>
        [JsonPropertyName("qty_multiplier")]
        public double QtyMultiplier { get; set; }

        /// <summary>
        /// Валюта шага цены
        /// </summary>
        [JsonPropertyName("step_price_currency")]
        public string StepPriceCurrency { get; set; }

        /// <summary>
        /// SEDOL
        /// </summary>
        [JsonPropertyName("sedol_code")]
        public string SEDOLCode { get; set; }

        /// <summary>
        /// CFI
        /// </summary>
        [JsonPropertyName("cfi_code")]
        public string CFICode { get; set; }

        /// <summary>
        /// RIC
        /// </summary>
        [JsonPropertyName("ric_code")]
        public string RICCode { get; set; }

        /// <summary>
        /// Дата оферты
        /// Представление в виде числа `YYYYMMDD`
        /// </summary>
        [JsonPropertyName("buybackdate")]
        public int BuybackDate { get; set; }

        /// <summary>
        /// Цена оферты
        /// </summary>
        [JsonPropertyName("buybackprice")]
        public double BuybackPrice { get; set; }

        /// <summary>
        /// Уровень листинга
        /// </summary>
        [JsonPropertyName("list_level")]
        public int ListLevel { get; set; }

        /// <summary>
        /// Точность количества
        /// </summary>
        [JsonPropertyName("qty_scale")]
        public int QtyScale { get; set; }

        /// <summary>
        /// Доходность по предыдущей оценке
        /// </summary>
        [JsonPropertyName("yieldatprevwaprice")]
        public double YieldAtPrevWaPrice { get; set; }

        /// <summary>
        /// Регистрационный номер
        /// </summary>
        [JsonPropertyName("regnumber")]
        public string RegNumber { get; set; }

        /// <summary>
        /// Валюта торгов
        /// </summary>
        [JsonPropertyName("trade_currency")]
        public string TradeCurrency { get; set; }

        /// <summary>
        /// Точность количества котируемой валюты
        /// </summary>
        [JsonPropertyName("second_curr_qty_scale")]
        public int SecondCurrQtyScale { get; set; }

        /// <summary>
        /// Точность количества базовой валюты
        /// </summary>
        [JsonPropertyName("first_curr_qty_scale")]
        public int FirstCurrQtyScale { get; set; }

        /// <summary>
        /// Накопленный купонный доход
        /// </summary>
        [JsonPropertyName("accruedint")]
        public double Accruedint { get; set; }

        /// <summary>
        /// Код деривативного контракта в формате QUIK
        /// </summary>
        [JsonPropertyName("stock_name")]
        public string StockName { get; set; }

        /// <summary>
        /// Дата выплаты купона
        /// Представление в виде числа `YYYYMMDD`
        /// </summary>
        [JsonPropertyName("nextcoupon")]
        public int NextCoupon { get; set; }

        /// <summary>
        /// Длительность купона
        /// </summary>
        [JsonPropertyName("couponperiod")]
        public int CouponPeriod { get; set; }

        /// <summary>
        /// Текущий код расчетов для инструмента
        /// </summary>
        [JsonPropertyName("settlecode")]
        public string SettleCode { get; set; }

        /// <summary>
        /// Дата экспирации
        /// Представление в виде числа `YYYYMMDD`
        /// </summary>
        [JsonPropertyName("exp_date")]
        public int ExpDate { get; set; }

        /// <summary>
        /// Дата расчетов
        /// Представление в виде числа `YYYYMMDD`
        /// </summary>
        [JsonPropertyName("settle_date")]
        public int SettleDate { get; set; }

        ///// <summary>
        ///// Ноги составного инструмента в формате leg_<N>
        ///// понка непонятно как реализовывать
        ///// </summary>
        //[JsonPropertyName("legs")]
        //public table Legs { get; set; }
    }
}