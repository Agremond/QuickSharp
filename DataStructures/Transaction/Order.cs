// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures.Transaction
{
    /// <summary>
    /// Описание параметров Таблицы заявок
    /// </summary>
    public class Order : IWithLuaTimeStamp
    {
        [JsonPropertyName("lua_timestamp")]
        public double LuaTimeStamp { get; internal set; }

        /// <summary>
        /// Номер заявки в торговой системе
        /// </summary>
        [JsonPropertyName("order_num")]
        public double OrderNum { get; set; }

        /// <summary>
        /// Поручение/комментарий, обычно: код клиента/номер поручения
        /// </summary>
        [JsonPropertyName("brokerref")]
        public string Comment { get; set; }

        /// <summary>
        /// Идентификатор трейдера
        /// </summary>
        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Количество в лотах
        /// </summary>
        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        /// <summary>
        /// Остаток
        /// </summary>
        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Объем в денежных средствах
        /// </summary>
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Накопленный купонный доход
        /// </summary>
        [JsonPropertyName("accruedint")]
        public decimal AccruedInterest { get; set; }

        /// <summary>
        /// Доходность
        /// </summary>
        [JsonPropertyName("yield")]
        public decimal Yield { get; set; }

        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        [JsonPropertyName("trans_id")]
        public long TransID { get; set; }

        /// <summary>
        /// Код клиента
        /// </summary>
        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Цена выкупа
        /// </summary>
        [JsonPropertyName("price2")]
        public decimal Price2 { get; set; }

        /// <summary>
        /// Код расчетов
        /// </summary>
        [JsonPropertyName("settlecode")]
        public string Settlecode { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [JsonPropertyName("uid")]
        public long Uid { get; set; }

        /// <summary>
        /// Идентификатор пользователя, снявшего заявку
        /// </summary>
        [JsonPropertyName("canceled_uid")]
        public long CanceledUserID { get; set; }

        /// <summary>
        /// Код биржи в торговой системе
        /// </summary>
        [JsonPropertyName("exchange_code")]
        public string ExchangeCode { get; set; }

        /// <summary>
        /// Время активации
        /// </summary>
        [JsonPropertyName("activation_time")]
        public decimal ActivationTime { get; set; }

        /// <summary>
        /// Номер заявки в торговой системе
        /// </summary>
        [JsonPropertyName("linkedorder")]
        public long Linkedorder { get; set; }

        /// <summary>
        /// Дата окончания срока действия заявки
        /// </summary>
        [JsonPropertyName("expiry")]
        public decimal Expiry { get; set; }

        /// <summary>
        /// Код бумаги заявки
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код класса заявки
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Дата и время
        /// </summary>
        [JsonPropertyName("datetime")]
        public QuikDateTime Datetime { get; set; }

        /// <summary>
        /// Дата и время снятия заявки
        /// </summary>
        [JsonPropertyName("withdraw_datetime")]
        public QuikDateTime WithdrawDatetime { get; set; }

        /// <summary>
        /// Идентификатор расчетного счета/кода в клиринговой организации
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccID { get; set; }

        /// <summary>
        /// Способ указания объема заявки. Возможные значения: 
        /// «0» – по количеству, 
        /// «1» – по объему
        /// </summary>
        [JsonPropertyName("value_entry_type")]
        public int ValueEntryType { get; set; }

        /// <summary>
        /// Срок РЕПО, в календарных днях
        /// </summary>
        [JsonPropertyName("repoterm")]
        public decimal Repoterm { get; set; }

        /// <summary>
        /// Сумма РЕПО на текущую дату. Отображается с точностью 2 знака
        /// </summary>
        [JsonPropertyName("repovalue")]
        public decimal Repovalue { get; set; }

        /// <summary>
        /// Объём сделки выкупа РЕПО. Отображается с точностью 2 знака
        /// </summary>
        [JsonPropertyName("repo2value")]
        public decimal Repo2Value { get; set; }

        /// <summary>
        /// Остаток суммы РЕПО за вычетом суммы привлеченных или предоставленных по сделке РЕПО денежных средств в неисполненной части заявки, по состоянию на текущую дату. Отображается с точностью 2 знака
        /// </summary>
        [JsonPropertyName("repo_value_balance")]
        public decimal RepoValueBalance { get; set; }

        /// <summary>
        /// Начальный дисконт, в %
        /// </summary>
        [JsonPropertyName("start_discount")]
        public decimal StartDiscount { get; set; }

        /// <summary>
        /// Причина отклонения заявки брокером
        /// </summary>
        [JsonPropertyName("reject_reason")]
        public string RejectReason { get; set; }

        /// <summary>
        /// Битовое поле для получения специфических параметров с западных площадок
        /// </summary>
        [JsonPropertyName("ext_order_flags")]
        public int ExtOrderFlags { get; set; }

        /// <summary>
        /// Минимально допустимое количество, которое можно указать в заявке по данному инструменту. Если имеет значение «0», значит ограничение по количеству не задано
        /// </summary>
        [JsonPropertyName("min_qty")]
        public int MinQty { get; set; }

        /// <summary>
        /// Тип исполнения заявки. Возможные значения:
        /// «0» – Значение не указано; 
        /// «1» – Немедленно или отклонить; 
        /// «2» – Поставить в очередь; 
        /// «3» – Снять остаток; 
        /// «4» – До снятия; 
        /// «5» – До даты; 
        /// «6» – В течение сессии; 
        /// «7» – Открытие; 
        /// «8» – Закрытие; 
        /// «9» – Кросс; 
        /// «11» – До следующей сессии; 
        /// «13» – До отключения; 
        /// «15» – До времени; 
        /// «16» – Следующий аукцион; 
        /// </summary>
        [JsonPropertyName("exec_type")]
        public int ExecType { get; set; }

        /// <summary>
        /// Поле для получения параметров по западным площадкам. Если имеет значение «0», значит значение не задано
        /// </summary>
        [JsonPropertyName("side_qualifier")]
        public int SideQualifier { get; set; }

        /// <summary>
        /// Поле для получения параметров по западным площадкам. Если имеет значение «0», значит значение не задано
        /// </summary>
        [JsonPropertyName("acnt_type")]
        public int AcntType { get; set; }

        /// <summary>
        /// Роль в исполнении заявки. Возможные значения:
        /// «0» – Не определено; 
        /// «1» – Agent; 
        /// «2» – Principal; 
        /// «3» – Riskless principal; 
        /// «4» – CFG give up; 
        /// «5» – Cross as agent; 
        /// «6» – Matched Principal; 
        /// «7» – Proprietary; 
        /// «8» – Individual; 
        /// «9» – Agent for other member; 
        /// «10» – Mixed; 
        /// «11» – Market maker;
        /// </summary>
        [JsonPropertyName("capacity")]
        public decimal Capacity { get; set; }

        /// <summary>
        /// Поле для получения параметров по западным площадкам. Если имеет значение «0», значит значение не задано
        /// </summary>
        [JsonPropertyName("passive_only_order")]
        public decimal PassiveOnlyOrder { get; set; }

        /// <summary>
        /// Видимое количество. Параметр айсберг-заявок, для обычных заявок выводится значение: «0»
        /// </summary>
        [JsonPropertyName("visible")]
        public decimal Visible { get; set; }

        /// <summary>
        /// Средняя цена приобретения. Актуально, когда заявка выполнилась частями
        /// </summary>
        [JsonPropertyName("awg_price")]
        public decimal AwgPrice { get; set; }

        /// <summary>
        /// Время окончания срока действия заявки в формате "ЧЧММСС DESIGNTIMESP=19552". Для GTT-заявок, используется вместе со сроком истечения заявки (Expiry)
        /// </summary>
        [JsonPropertyName("expiry_time")]
        public decimal ExpiryTime { get; set; }

        /// <summary>
        /// Номер ревизии заявки. Используется, если заявка была заменена с сохранением номера
        /// </summary>
        [JsonPropertyName("revision_number")]
        public decimal RevisionNumber { get; set; }

        /// <summary>
        /// Валюта цены заявки
        /// </summary>
        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; }

        /// <summary>
        /// Расширенный статус заявки. Возможные значения: 
        /// «0» (по умолчанию) – не определено; 
        /// «1» – заявка активна; 
        /// «2» – заявка частично исполнена; 
        /// «3» – заявка исполнена; 
        /// «4» – заявка отменена; 
        /// «5» – заявка заменена; 
        /// «6» – заявка в состоянии отмены; 
        /// «7» – заявка отвергнута; 
        /// «8» – приостановлено исполнение заявки; 
        /// «9» – заявка в состоянии регистрации; 
        /// «10» – заявка снята по времени действия; 
        /// «11» – заявка в состоянии замены
        /// </summary>
        [JsonPropertyName("ext_order_status")]
        public decimal ExtOrderStatus { get; set; }

        /// <summary>
        /// UID пользователя-менеджера, подтвердившего заявку при работе в режиме с подтверждениями
        /// </summary>
        [JsonPropertyName("accepted_uid")]
        public long AcceptedUID { get; set; }

        /// <summary>
        /// Исполненный объем заявки в валюте цены для частично или полностью исполненных заявок
        /// </summary>
        [JsonPropertyName("filled_value")]
        public decimal FilledValue { get; set; }

        /// <summary>
        /// Внешняя ссылка, используется для обратной связи с внешними системами
        /// </summary>
        [JsonPropertyName("extref")]
        public string ExtRef { get; set; }

        /// <summary>
        /// Валюта расчетов по заявке
        /// </summary>
        [JsonPropertyName("settle_currency")]
        public string SettleCurrency { get; set; }

        /// <summary>
        /// UID пользователя, от имени которого выставлена заявка
        /// </summary>
        [JsonPropertyName("on_behalf_of_uid")]
        public long OnBehalfOfUID { get; set; }

        /// <summary>
        /// Квалификатор клиента, от имени которого выставлена заявка. Возможные значения: 
        /// «0» – не определено; 
        /// «1» – Natural Person; 
        /// «3» – Legal Entity
        /// </summary>
        [JsonPropertyName("client_qualifier")]
        public int ClientQualifier { get; set; }

        /// <summary>
        /// Краткий идентификатор клиента, от имени которого выставлена заявка
        /// </summary>
        [JsonPropertyName("client_short_code")]
        public long ClientShortCode { get; set; }

        /// <summary>
        /// Квалификатор принявшего решение о выставлении заявки. Возможные значения: 
        /// «0» – не определено; 
        /// «1» – Natural Person; 
        /// «2» – Algorithm
        /// </summary>
        [JsonPropertyName("investment_decision_maker_qualifier")]
        public long InvestmentDecisionMakerQualifier { get; set; }

        /// <summary>
        /// Краткий идентификатор принявшего решение о выставлении заявки
        /// </summary>
        [JsonPropertyName("investment_decision_maker_short_code")]
        public long InvestmentDecisionMakerShortCode { get; set; }

        /// <summary>
        /// Квалификатор трейдера, исполнившего заявку. Возможные значения: 
        /// «0» – не определено; 
        /// «1» – Natural Person; 
        /// «2» – Algorithm
        /// </summary>
        [JsonPropertyName("executing_trader_qualifier")]
        public long ExecutingTraderQualifier { get; set; }

        /// <summary>
        /// Краткий идентификатор трейдера, исполнившего заявку
        /// </summary>
        [JsonPropertyName("executing_trader_short_code")]
        public long ExecutingTraderShortCode { get; set; }

        /// <summary>
        /// Дата расчетов. Для свопов дата расчетов первой части операции своп
        /// </summary>
        [JsonPropertyName("settle_date")]
        public long SettleDate { get; set; }

        /// <summary>
        /// Дата расчетов второй части операции своп
        /// </summary>
        [JsonPropertyName("settle_date2")]
        public long SettleDate2 { get; set; }

        /// <summary>
        /// Дата, начиная с которой допускается совершение валютирования. Заполняется только для контрактов типа «FLEX FORWARD» (см. параметр operation_type)
        /// </summary>
        [JsonPropertyName("start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// Тип совершаемой операции. Возможные значения: 
        /// «-1» – «NOT_DEFINED»; 
        /// «0» – «SPOT»; 
        /// «1» – «FORWARD»; 
        /// «2» – «SWAP»; 
        /// «6» – «NDF»; 
        /// «7» – «FLEX FORWARD»
        /// </summary>
        [JsonPropertyName("operation_type")]
        public int OperationType { get; set; }

        /// <summary>
        /// Количество второй части операции своп
        /// </summary>
        [JsonPropertyName("qty2")]
        public int Quantity2 { get; set; }

        /// <summary>
        /// Объем второй части операции своп
        /// </summary>
        [JsonPropertyName("value2")]
        public decimal Value2 { get; set; }

        /// <summary>
        /// Видимая часть в общей сумме заявки в %
        /// </summary>
        [JsonPropertyName("visibility_factor")]
        public decimal VisibilityFactor { get; set; }

        /// <summary>
        /// Сумма РЕПО видимой части (точность валюты расчетов заявки/инструмента)
        /// </summary>
        [JsonPropertyName("visible_repo_value")]
        public decimal VisibleRepoValue { get; set; }

        /// <summary>
        /// Идентификатор торговой сессии. Возможные значения:
        /// «0» –«Не определено»; 
        /// «1» –«Основная сессия»; 
        /// «2» –«Дополнительная сессия»; 
        /// «3» –«Итоги дня»
        /// </summary>
        [JsonPropertyName("trading_session")]
        public int TradingSession { get; set; }

        /// <summary>
        /// Тип ввода значения цены заявки. Возможные значения:
        /// «1» – «По цене»; 
        /// «2» – «По доходности»; 
        /// «3» - «По средневзвешенной цене»
        /// </summary>
        [JsonPropertyName("price_entry_type")]
        public int PriceEntryType { get; set; }

        /// <summary>
        /// Код инструмента, являющийся приоритетным обеспечением
        /// </summary>
        [JsonPropertyName("lseccode")]
        public string LSecCode { get; set; }

        /// <summary>
        /// Идентификатор индикативной ставки
        /// </summary>
        [JsonPropertyName("benchmark")]
        public string Benchmark { get; set; }

        /// <summary>
        /// Внешнее количество
        /// </summary>
        [JsonPropertyName("external_qty")]
        public decimal ExternalQuantity { get; set; }

        /// <summary>
        /// Тип операции - Buy или Sell
        /// </summary>
        [JsonIgnore]
        public Operation Operation { get; set; }

        /// <summary>
        /// Состояние заявки.
        /// </summary>
        [JsonIgnore]
        public State State { get; set; }

        private OrderTradeFlags _flags;

        /// <summary>
        /// Набор битовых флагов
        /// http://help.qlua.org/ch9_1.htm
        /// </summary>
        [JsonPropertyName("flags")]
        public OrderTradeFlags Flags
        {
            get { return _flags; }
            set
            {
                _flags = value;
                ParseFlags();
            }
        }

        private void ParseFlags()
        {
            Operation = Flags.HasFlag(OrderTradeFlags.IsSell) ? Operation.Sell : Operation.Buy;

            State = Flags.HasFlag(OrderTradeFlags.Active)
                ? State.Active
                : (Flags.HasFlag(OrderTradeFlags.Canceled)
                    ? State.Canceled
                    : State.Completed);
        }
    }
}