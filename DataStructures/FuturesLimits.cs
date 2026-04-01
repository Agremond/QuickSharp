// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// При получении изменений ограничений по срочному рынку функция возвращает таблицу Lua "Ограничения по клиентским счетам" с параметрами
    /// </summary>
    public class FuturesLimits
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("trdaccid")]
        public string TrdAccId { get; set; }

        /// <summary>
        /// Тип лимита. Возможные значения:
        /// «0» – «Денежные средства»,
        /// «1» – «Залоговые денежные средства»,
        /// «2» – «Всего»,
        /// «3» – «Клиринговые рубли»,
        /// «4» – «Клиринговые залоговые рубли»,
        /// «5» – «Лимит открытых позиций на спот-рынке»
        /// </summary>
        [JsonPropertyName("limit_type")]
        public int LimitType { get; set; }

        /// <summary>
        /// Коэффициент ликвидности
        /// </summary>
        [JsonPropertyName("liquidity_coef")]
        public double LiquidityCoef { get; set; }

        /// <summary>
        /// Предыдущий лимит открытых позиций на спот-рынке
        /// </summary>
        [JsonPropertyName("cbp_prev_limit")]
        public double CbpPrevLimit { get; set; }

        /// <summary>
        /// Лимит открытых позиций
        /// </summary>
        [JsonPropertyName("cbplimit")]
        public double CbpLimit { get; set; }

        /// <summary>
        /// Текущие чистые позиции
        /// </summary>
        [JsonPropertyName("cbplused")]
        public double CbpLUsed { get; set; }

        /// <summary>
        /// Плановые чистые позиции
        /// </summary>
        [JsonPropertyName("cbplplanned")]
        public double CbpLPlanned { get; set; }

        /// <summary>
        /// Вариационная маржа
        /// </summary>
        [JsonPropertyName("varmargin")]
        public double VarMargin { get; set; }

        /// <summary>
        /// Накопленный купонный доход
        /// </summary>
        [JsonPropertyName("accruedint")]
        public double AccruedInt { get; set; }

        /// <summary>
        /// Текущие чистые позиции (под заявки)
        /// </summary>
        [JsonPropertyName("cbplused_for_orders")]
        public double CbpLUsedForOrders { get; set; }

        /// <summary>
        /// Текущие чистые позиции (под открытые позиции)
        /// </summary>
        [JsonPropertyName("cbplused_for_positions")]
        public double CbpLUsedForPositions { get; set; }

        /// <summary>
        /// Премия по опционам
        /// </summary>
        [JsonPropertyName("options_premium")]
        public double OptionsPremium { get; set; }

        /// <summary>
        /// Биржевые сборы
        /// </summary>
        [JsonPropertyName("ts_comission")]
        public double TSComission { get; set; }

        /// <summary>
        /// Коэффициент клиентского гарантийного обеспечения
        /// </summary>
        [JsonPropertyName("kgo")]
        public double KGO { get; set; }

        /// <summary>
        /// Валюта, в которой транслируется ограничение
        /// </summary>
        [JsonPropertyName("currcode")]
        public string CurrCode { get; set; }

        /// <summary>
        /// Реально начисленная в ходе клиринга вариационная маржа. Отображается с точностью до 2 двух знаков.
        /// При этом, в поле «varmargin» транслируется вариационная маржа, рассчитанная с учетом установленных границ изменения цены
        /// </summary>
        [JsonPropertyName("real_varmargin")]
        public double RealVarMargin { get; set; }

        /// <summary>
        /// Уровень риска клиента
        /// «0» – (пусто, по умолчанию), уровень риска не указан; 
        /// «1» – КНУР(клиент с начальным уровнем риска); 
        /// «2» – КСУР(клиент со стандартным уровнем риска); 
        /// «3» – КПУР(клиент с повышенным уровнем риска); 
        /// «4» – КОУР(клиент с особым уровнем риска)
        /// </summary>
        [JsonPropertyName("risk_level")]
        public int RiskLevel { get; set; }

        /// <summary>
        /// Гарантийное обеспечение без учета заявок, с учетом текущего реализованного риска (с учетом текущих цен) по риск-параметрам на начало сессии
        /// </summary>
        [JsonPropertyName("go_without_orders")]
        public double GOWithoutOrders { get; set; }

        /// <summary>
        /// Гарантийное обеспечение без учета заявок с учетом текущих риск-параметров и рыночных данных, в ден. выражении. Отображается с точностью до 2 знаков после разделителя
        /// </summary>
        [JsonPropertyName("go_planned")]
        public double GOPlanned { get; set; }

        /// <summary>
        /// Индикативная вариационная маржа с учетом текущего индикативного курса валют, руб. (рассчитывается аналогично текущей индикативной вариационной марже, учитывает в т.ч. вариационную маржу по закрытым позициям). Рассчитывается торговой системой срочного рынка Московской Биржи. Отображается с точностью до 2 знаков после разделителя
        /// </summary>
        [JsonPropertyName("indicative_varmargin")]
        public double IndicativeVarMargin { get; set; }

        // ReSharper restore InconsistentNaming
    }
}