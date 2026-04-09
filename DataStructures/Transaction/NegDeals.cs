using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures.Transaction
{
    /// <summary>
    /// Описание параметров Таблицы заявок на внебиржевые сделки
    /// </summary>
    public class NegDeals : IWithLuaTimeStamp
    {
        [JsonPropertyName("lua_timestamp")]
        public double LuaTimeStamp { get; internal set; }

        /// <summary>
        /// Номер
        /// </summary>
        [JsonPropertyName("neg_deal_num")]
        public double NegDealNumber { get; set; }

        /// <summary>
        /// Время выставления заявки
        /// </summary>
        [JsonPropertyName("neg_deal_time")]
        public double NegDealTime { get; set; }

        private OrderTradeFlags _flags;

        /// <summary>
        /// Набор битовых флагов
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

        /// <summary>
        /// Поручение/комментарий, обычно: код клиента/номер поручения
        /// </summary>
        [JsonPropertyName("brokerref")]
        public string Comment { get; set; }

        /// <summary>
        /// Трейдер
        /// </summary>
        [JsonPropertyName("userid")]
        public string UserId { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firmid")]
        public string FirmId { get; set; }

        /// <summary>
        /// Трейдер партнера
        /// </summary>
        [JsonPropertyName("cpuserid")]
        public string CpUserId { get; set; }

        /// <summary>
        /// Код фирмы партнера
        /// </summary>
        [JsonPropertyName("cpfirmid")]
        public string CpFirmId { get; set; }

        /// <summary>
        /// Счет
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        [JsonPropertyName("qty")]
        public int Quantity { get; set; }

        /// <summary>
        /// Ссылка
        /// </summary>
        [JsonPropertyName("matchref")]
        public string MatchRef { get; set; }

        /// <summary>
        /// Код расчетов
        /// </summary>
        [JsonPropertyName("settlecode")]
        public string Settlecode { get; set; }

        /// <summary>
        /// Доходность
        /// </summary>
        [JsonPropertyName("yield")]
        public decimal Yield { get; set; }

        /// <summary>
        /// Купонный процент (Странно. В дригих классах это назвается - Накопленный купонный доход)
        /// </summary>
        [JsonPropertyName("accruedint")]
        public decimal AccruedInterest { get; set; }

        /// <summary>
        /// Объем (в денежных средствах)
        /// </summary>
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        /// <summary>
        /// Цена выкупа
        /// </summary>
        [JsonPropertyName("price2")]
        public decimal Price2 { get; set; }

        /// <summary>
        /// Ставка РЕПО (%)
        /// </summary>
        [JsonPropertyName("reporate")]
        public decimal RepoRate { get; set; }

        /// <summary>
        /// Ставка возмещения (%)
        /// </summary>
        [JsonPropertyName("refundrate")]
        public decimal RefundRate { get; set; }

        /// <summary>
        /// ID транзакции
        /// </summary>
        [JsonPropertyName("trans_id")]
        public long TransID { get; set; }

        /// <summary>
        /// Код клиента
        /// </summary>
        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Тип ввода заявки РЕПО. Возможные значения:
        /// «0» – «Не определен»; 
        /// «1» – «Цена1+Ставка»; 
        /// «2» – «Ставка+ Цена2»; 
        /// «3» – «Цена1+Цена2»; 
        /// «4» – «Сумма РЕПО + Количество»; 
        /// «5» – «Сумма РЕПО + Дисконт»; 
        /// «6» – «Количество + Дисконт»; 
        /// «7» – «Сумма РЕПО»; 
        /// «8» – «Количество» 
        /// </summary>
        [JsonPropertyName("repoentry")]
        public int RepoEntry { get; set; }

        /// <summary>
        /// Сумма РЕПО
        /// </summary>
        [JsonPropertyName("repovalue")]
        public decimal Repovalue { get; set; }

        /// <summary>
        /// Объём выкупа РЕПО
        /// </summary>
        [JsonPropertyName("repo2value")]
        public decimal Repo2Value { get; set; }

        /// <summary>
        /// Срок РЕПО (в календарных днях)
        /// </summary>
        [JsonPropertyName("repoterm")]
        public decimal Repoterm { get; set; }

        /// <summary>
        /// Начальный дисконт, в %
        /// </summary>
        [JsonPropertyName("start_discount")]
        public decimal StartDiscount { get; set; }

        /// <summary>
        /// Нижний дисконт (%)
        /// </summary>
        [JsonPropertyName("lower_discount")]
        public decimal LowerDiscount { get; set; }

        /// <summary>
        /// Верхний дисконт (%)
        /// </summary>
        [JsonPropertyName("upper_discount")]
        public decimal UpperDiscount { get; set; }

        /// <summary>
        /// Блокировка обеспечения («Да»/«Нет»)
        /// </summary>
        [JsonPropertyName("block_securities")]
        public decimal BlockSecurities { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        [JsonPropertyName("uid")]
        public string UId { get; set; }

        /// <summary>
        /// Время снятия заявки
        /// </summary>
        [JsonPropertyName("withdraw_time")]
        public int WithdrawTime { get; set; }

        /// <summary>
        /// Дата выставления заявки
        /// </summary>
        [JsonPropertyName("neg_deal_date")]
        public int NegDealDate { get; set; }

        /// <summary>
        /// Остаток
        /// </summary>
        [JsonPropertyName("balance")]
        public int Balance { get; set; }

        /// <summary>
        /// Сумма РЕПО первоначальная
        /// </summary>
        [JsonPropertyName("origin_repovalue")]
        public decimal OriginRepoValue { get; set; }

        /// <summary>
        /// Количество первоначальное
        /// </summary>
        [JsonPropertyName("origin_qty")]
        public decimal OriginQuantity { get; set; }

        /// <summary>
        /// Процент дисконта первоначальный
        /// </summary>
        [JsonPropertyName("origin_discount")]
        public decimal OriginDiscount { get; set; }

        /// <summary>
        /// Дата активации заявки
        /// </summary>
        [JsonPropertyName("neg_deal_activation_date")]
        public int NegDealActivationDate { get; set; }

        /// <summary>
        /// Время активации заявки
        /// </summary>
        [JsonPropertyName("neg_deal_activation_time")]
        public int NegDealActivationTimne { get; set; }

        /// <summary>
        /// Встречная безадресная заявка
        /// </summary>
        [JsonPropertyName("quoteno")]
        public long QuoteNo { get; set; }

        /// <summary>
        /// Валюта расчетов
        /// </summary>
        [JsonPropertyName("settle_currency")]
        public string SettleCurrency { get; set; }

        /// <summary>
        /// Код инструмента
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Идентификатор расчетного счета/кода в клиринговой организации
        /// </summary>
        [JsonPropertyName("bank_acc_id")]
        public string BankAccID { get; set; }

        /// <summary>
        /// Дата снятия адресной заявки в формате «ГГГГММДД»
        /// </summary>
        [JsonPropertyName("withdraw_date")]
        public int WithdrawDate { get; set; }

        /// <summary>
        /// Номер предыдущей заявки. Отображается с точностью «0»
        /// </summary>
        [JsonPropertyName("linkedorder")]
        public long Linkedorder { get; set; }

        /// <summary>
        /// Дата и время активации заявки
        /// </summary>
        [JsonPropertyName("activation_date_time")]
        public QuikDateTime ActivationDatetime { get; set; }

        /// <summary>
        /// Дата и время снятия заявки
        /// </summary>
        [JsonPropertyName("withdraw_date_time")]
        public QuikDateTime WithdrawDatetime { get; set; }

        /// <summary>
        /// Дата и время заявки
        /// </summary>
        [JsonPropertyName("datetime")]
        public QuikDateTime Datetime { get; set; }

        /// <summary>
        /// Код инструмента, являющийся приоритетным обеспечением
        /// </summary>
        [JsonPropertyName("lseccode")]
        public string LSecCode { get; set; }

        /// <summary>
        /// Идентификатор пользователя, снявшего заявку
        /// </summary>
        [JsonPropertyName("canceled_uid")]
        public long CanceledUserID { get; set; }

        /// <summary>
        /// Системная ссылка
        /// </summary>
        [JsonPropertyName("system_ref")]
        public string SystemRef { get; set; }

        /// <summary>
        /// Валюта, в которой указана цена заявки
        /// </summary>
        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; }

        /// <summary>
        /// Биржевой номер заявки
        /// </summary>
        [JsonPropertyName("order_exchange_code")]
        public string OrderExchangeCode { get; set; }

        /// <summary>
        /// Внешняя ссылка, используется для обратной связи с внешними системами
        /// </summary>
        [JsonPropertyName("extref")]
        public string ExtRef { get; set; }

        /// <summary>
        /// Период торговой сессии, в которую была подана заявка
        /// </summary>
        [JsonPropertyName("period")]
        public decimal Period { get; set; }

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
        /// Идентификатор индикативной ставки
        /// </summary>
        [JsonPropertyName("benchmark")]
        public string Benchmark { get; set; }

        /// <summary>
        /// Набор битовых флагов. Возможные значения: 
        /// бит 0 (0x1) – Валюты на адресной заявке заполнены в соответствии с общими правилами заполнения валют; 
        /// бит 1 (0x2) – Признак заявки типа РЕПО с открытой датой
        /// </summary>
        [JsonPropertyName("ext_negdeal_flags")]
        public NegDealsFlags ExtNegDealFlags { get; set; }

        /// <summary>
        /// День Т+1 для сделок РЕПО с Открытой датой
        /// </summary>
        [JsonPropertyName("open_repo2date")]
        public int OpenRepo2Date { get; set; }

        /// <summary>
        /// Стоимость выкупа РЕПО с открытой датой в день T+1
        /// </summary>
        [JsonPropertyName("open_repo2value")]
        public decimal OpenRepo2Value { get; set; }

        /// <summary>
        /// Причина отклонения заявки
        /// </summary>
        [JsonPropertyName("reject_reason")]
        public string RejectReason { get; set; }


        /// <summary>
        /// Тип операции - Buy или Sell
        /// </summary>
        [JsonIgnore]
        public Operation Operation { get; set; }

        /// <summary>
        /// Состояние заявки.
        /// </summary>
        [JsonIgnore]
        public State State { get; private set; }

    }
}