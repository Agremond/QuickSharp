// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Параметры таблицы "Клиентский портфель", соответствующих идентификатору участника торгов «firmId» и коду клиента «clientCode», Возвращаемой функцией GetPortfolioInfo
    /// </summary>
    public class PortfolioInfo
    {
        /// <summary>
        /// Тип клиента
        /// Признак использования схемы кредитования с контролем текущей стоимости активов. Возможные значения:
        /// «МЛ» – используется схема ведения позиции «по плечу», «плечо» рассчитано по значению Входящего лимита
        /// «МП» – используется схема ведения позиции «по плечу», «плечо» указано явным образом
        /// «МОП» – используется схема ведения позиции «лимит на открытую позицию»
        /// «МД» – используется схема ведения позиции «по дисконтам»
        /// \пусто\ – используется схема ведения позиции «по лимитам»
        /// </summary>
        [JsonPropertyName("is_leverage")]
        public string IsLeverage { get; set; }

        /// <summary>
        /// Вход. активы
        /// Оценка собственных средств клиента до начала торгов
        /// </summary>
        [JsonPropertyName("in_assets")]
        public string InAssets { get; set; }

        /// <summary>
        /// Плечо
        /// Плечо. Если не задано явно, то отношение Входящего лимита к Входящим активам
        /// </summary>
        [JsonPropertyName("leverage")]
        public string Leverage { get; set; }

        /// <summary>
        /// Вход. лимит
        /// Оценка максимальной величины заемных средств до начала торгов
        /// </summary>
        [JsonPropertyName("open_limit")]
        public string OpenLimit { get; set; }

        /// <summary>
        /// Шорты
        /// Оценка стоимости коротких позиций. Значение всегда отрицательное
        /// </summary>
        [JsonPropertyName("val_short")]
        public string ValShort { get; set; }

        /// <summary>
        /// Лонги
        /// Оценка стоимости длинных позиций
        /// </summary>
        [JsonPropertyName("val_long")]
        public string ValLong { get; set; }

        /// <summary>
        /// Лонги МО
        /// Оценка стоимости длинных позиций по маржинальным бумагам, принимаемым в обеспечение
        /// </summary>
        [JsonPropertyName("val_long_margin")]
        public string ValLongMargin { get; set; }

        /// <summary>
        /// Лонги О
        /// Оценка стоимости длинных позиций по немаржинальным бумагам, принимаемым в обеспечение
        /// </summary>
        [JsonPropertyName("val_long_asset")]
        public string ValLongAsset { get; set; }

        /// <summary>
        /// Тек. активы
        /// Оценка собственных средств клиента по текущим позициям и ценам
        /// </summary>
        [JsonPropertyName("assets")]
        public string Assets { get; set; }

        /// <summary>
        /// Текущее плечо
        /// </summary>
        [JsonPropertyName("cur_leverage")]
        public string CurLeverage { get; set; }

        /// <summary>
        /// Ур. маржи
        /// Уровень маржи, в процентах
        /// </summary>
        [JsonPropertyName("margin")]
        public string Margin { get; set; }

        /// <summary>
        /// Тек. лимит
        /// Текущая оценка максимальной величины заемных средств
        /// </summary>
        [JsonPropertyName("lim_all")]
        public string LimAll { get; set; }

        /// <summary>
        /// ДостТекЛимит
        /// Оценка величины заемных средств, доступных для дальнейшего открытия позиций
        /// </summary>
        [JsonPropertyName("av_lim_all")]
        public string AvLimAll { get; set; }

        /// <summary>
        /// Блок. покупка
        /// Оценка стоимости активов в заявках на покупку
        /// </summary>
        [JsonPropertyName("locked_buy")]
        public string LockedBuy { get; set; }

        /// <summary>
        /// Блок. пок. маржин.
        /// Оценка стоимости активов в заявках на покупку маржинальных бумаг, принимаемых в обеспечение
        /// </summary>
        [JsonPropertyName("locked_buy_margin")]
        public string LockedBuyMargin { get; set; }

        /// <summary>
        /// Блок.пок. обесп.
        /// Оценка стоимости активов в заявках на покупку немаржинальных бумаг, принимаемых в обеспечение
        /// </summary>
        [JsonPropertyName("locked_buy_asset")]
        public string LockedBuyAsset { get; set; }

        /// <summary>
        /// Блок. продажа
        /// Оценка стоимости активов в заявках на продажу маржинальных бумаг
        /// </summary>
        [JsonPropertyName("locked_sell")]
        public string LockedSell { get; set; }

        /// <summary>
        /// Блок. пок. немарж.
        /// Оценка стоимости активов в заявках на покупку немаржинальных бумаг
        /// </summary>
        [JsonPropertyName("locked_value_coef")]
        public string LockedValueCoef { get; set; }

        /// <summary>
        /// ВходСредства
        /// Оценка стоимости всех позиций клиента в ценах закрытия предыдущей торговой сессии, включая позиции по немаржинальным бумагам
        /// </summary>
        [JsonPropertyName("in_all_assets")]
        public string InAllAssets { get; set; }

        /// <summary>
        /// ТекСредства
        /// Текущая оценка стоимости всех позиций клиента
        /// </summary>
        [JsonPropertyName("all_assets")]
        public string AllAssets { get; set; }

        /// <summary>
        /// Прибыль/убытки
        /// Абсолютная величина изменения стоимости всех позиций клиента
        /// </summary>
        [JsonPropertyName("profit_loss")]
        public string ProfitLoss { get; set; }

        /// <summary>
        /// ПроцИзмен
        /// Относительная величина изменения стоимости всех позиций клиента
        /// </summary>
        [JsonPropertyName("rate_change")]
        public string RateChange { get; set; }

        /// <summary>
        /// На покупку
        /// Оценка денежных средств, доступных для покупки маржинальных бумаг
        /// </summary>
        [JsonPropertyName("lim_buy")]
        public string LimBuy { get; set; }

        /// <summary>
        /// На продажу
        /// Оценка стоимости маржинальных бумаг, доступных для продажи
        /// </summary>
        [JsonPropertyName("lim_sell")]
        public string LimSell { get; set; }

        /// <summary>
        /// НаПокупНеМаржин
        /// Оценка денежных средств, доступных для покупки немаржинальных бумаг
        /// </summary>
        [JsonPropertyName("lim_non_margin")]
        public string LimNonMargin { get; set; }

        /// <summary>
        /// НаПокупОбесп
        /// Оценка денежных средств, доступных для покупки бумаг, принимаемых в обеспечение
        /// </summary>
        [JsonPropertyName("lim_buy_asset")]
        public string LimBuyAsset { get; set; }

        /// <summary>
        /// Шорты (нетто)
        /// Оценка стоимости коротких позиций. При расчете не используется коэффициент дисконтирования
        /// </summary>
        [JsonPropertyName("val_short_net")]
        public string ValShortNet { get; set; }

        /// <summary>
        /// Сумма ден. остатков
        /// Сумма остатков по денежным средствам по всем лимитам, без учета средств, заблокированных под исполнение обязательств, выраженная в выбранной валюте расчета
        /// </summary>
        [JsonPropertyName("total_money_bal")]
        public string TotalMoneyBal { get; set; }

        /// <summary>
        /// Суммарно заблок.
        /// Cумма заблокированных средств со всех денежных лимитов клиента, пересчитанная в валюту расчетов через кросс-курсы на сервере
        /// </summary>
        [JsonPropertyName("total_locked_money")]
        public string TotalLockedMoney { get; set; }

        /// <summary>
        /// Сумма дисконтов
        /// Сумма дисконтов стоимости длинных (только по бумагам обеспечения) и коротких бумажных позиций, дисконтов корреляции между инструментами, а также дисконтов на задолженности по валютам, не покрытые бумажным обеспечением в этих же валютах
        /// </summary>
        [JsonPropertyName("haircuts")]
        public string Haircuts { get; set; }

        /// <summary>
        /// ТекАктБезДиск
        /// Суммарная величина денежных остатков, стоимости длинных позиций по бумагам обеспечения и стоимости коротких позиций, без учета дисконтирующих коэффициентов, без учета неттинга стоимости бумаг в рамках объединенной бумажной позиции и без учета корреляции между инструментами
        /// </summary>
        [JsonPropertyName("assets_without_hc")]
        public string AssetsWithoutHC { get; set; }

        /// <summary>
        /// Статус счета
        /// Отношение суммы дисконтов к текущим активам без учета дисконтов
        /// </summary>
        [JsonPropertyName("status_coef")]
        public string StatusCoef { get; set; }

        /// <summary>
        /// Вариац. маржа
        /// Текущая вариационная маржа по позициям клиента, по всем инструментам
        /// </summary>
        [JsonPropertyName("varmargin")]
        public string VarMargin { get; set; }

        /// <summary>
        /// ГО поз.
        /// Размер денежных средств, уплаченных под все открытые позиции на срочном рынке
        /// </summary>
        [JsonPropertyName("go_for_positions")]
        public string GOForPositions { get; set; }

        /// <summary>
        /// ГО заяв.
        /// Оценка стоимости активов в заявках на срочном рынке
        /// </summary>
        [JsonPropertyName("go_for_orders")]
        public string GOForOrders { get; set; }

        /// <summary>
        /// Активы/ГО
        /// Отношение ликвидационной стоимости портфеля к ГО по срочному рынку
        /// </summary>
        [JsonPropertyName("rate_futures")]
        public string RateFutures { get; set; }

        /// <summary>
        /// ПовышУрРиска
        /// Признак «квалифицированного» клиента, которому разрешено кредитование заемными средствами с плечом 1:3.
        /// Возможные значения: «ПовышУрРиска» – квалифицированный, /пусто/ – нет
        /// </summary>
        [JsonPropertyName("is_qual_client")]
        public string IsQualClient { get; set; }

        /// <summary>
        /// Сроч. счет
        /// Счет клиента на FORTS, в случае наличия объединенной позиции, иначе поле остается пустым
        /// </summary>
        [JsonPropertyName("is_futures")]
        public string IsFutures { get; set; }

        /// <summary>
        /// Парам. расч.
        /// Актуальные текущие параметры расчета для данной строки в формате «/Валюта/-/Идентификатор торговой сессии/». Пример: «SUR-EQTV»
        /// </summary>
        [JsonPropertyName("curr_TAG")]
        public string CurrTAG { get; set; }
    }
}