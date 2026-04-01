// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Описание параметров Таблицы позиций по клиентским счетам (фьючерсы):
    /// </summary>
    public class FuturesClientHolding : IWithLuaTimeStamp
    {
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string firmId { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("trdaccid")]
        public string trdAccId { get; set; }

        /// <summary>
        /// Код фьючерсного контракта
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string secCode { get; set; }

        /// <summary>
        /// Тип лимита. Возможные значения:
        /// «Основной счет»;
        /// «Клиентские и дополнительные счета»;
        /// «Все счета торг. членов»;
        /// </summary>
        [JsonPropertyName("type")]
        public string type { get; set; }

        /// <summary>
        /// Входящие длинные позиции
        /// </summary>
        [JsonPropertyName("startbuy")]
        public double startBuy { get; set; }

        /// <summary>
        /// Входящие короткие позиции
        /// </summary>
        [JsonPropertyName("startsell")]
        public double startSell { get; set; }

        /// <summary>
        /// Входящие чистые позиции
        /// </summary>
        [JsonPropertyName("startnet")]
        public double startNet { get; set; }

        /// <summary>
        /// Текущие длинные позиции
        /// </summary>
        [JsonPropertyName("todaybuy")]
        public double todayBuy { get; set; }

        /// <summary>
        /// Текущие короткие позиции
        /// </summary>
        [JsonPropertyName("todaysell")]
        public double todaySell { get; set; }

        /// <summary>
        /// Текущие чистые позиции
        /// </summary>
        [JsonPropertyName("totalnet")]
        public double totalNet { get; set; }

        /// <summary>
        /// Активные на покупку
        /// </summary>
        [JsonPropertyName("openbuys")]
        public double openBuys { get; set; }

        /// <summary>
        /// Активные на продажу
        /// </summary>
        [JsonPropertyName("opensells")]
        public double openSells { get; set; }

        /// <summary>
        /// Оценка текущих чистых позиций
        /// </summary>
        [JsonPropertyName("cbplused")]
        public double cbPlUsed { get; set; }

        /// <summary>
        /// Плановые чистые позиции
        /// </summary>
        [JsonPropertyName("cbplplanned")]
        public double cbpPPlanned { get; set; }

        /// <summary>
        /// Вариационная маржа
        /// </summary>
        [JsonPropertyName("varmargin")]
        public double varMargin { get; set; }

        /// <summary>
        /// Эффективная цена позиций
        /// </summary>
        [JsonPropertyName("avrposnprice")]
        public double avrPosnPrice { get; set; }

        /// <summary>
        /// Стоимость позиций
        /// </summary>
        [JsonPropertyName("positionvalue")]
        public double positionValue { get; set; }

        /// <summary>
        /// Реально начисленная в ходе клиринга вариационная маржа.
        /// Отображается с точностью до 2 двух знаков.
        /// При этом, в поле «varmargin» транслируется вариационная маржа, рассчитанная с учетом установленных границ изменения цены
        /// </summary>
        [JsonPropertyName("real_varmargin ")]
        public double realVarMargin { get; set; }

        /// <summary>
        /// Суммарная вариационная маржа по итогам основного клиринга начисленная по всем позициям.
        /// Отображается с точностью до 2 двух знаков
        /// </summary>
        [JsonPropertyName("total_varmargin ")]
        public double totalVarMargin { get; set; }

        /// <summary>
        /// Актуальный статус торговой сессии. Возможные значения: 
        /// «0» – не определено; 
        /// «1» – основная сессия; 
        /// «2» – начался промклиринг; 
        /// «3» – завершился промклиринг; 
        /// «4» – начался основной клиринг; 
        /// «5» – основной клиринг: новая сессия назначена; 
        /// «6» – завершился основной клиринг; 
        /// «7» – завершилась вечерняя сессия
        /// </summary>
        [JsonPropertyName("session_status ")]
        public int SessionStatus { get; set; }

        public double LuaTimeStamp { get; set; }
    }
}