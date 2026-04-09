// Copyright (c) 2014-2020 QuikSharp Authors https://github.com/finsight/QuikSharp/blob/master/AUTHORS.md. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures.Transaction
{
    /// <summary>
    /// Результат OnTransReply
    /// </summary>
    public class TransactionReply : IWithLuaTimeStamp
    {
        [JsonPropertyName("lua_timestamp")]
        public double LuaTimeStamp { get; internal set; }

        /// <summary>
        /// Пользовательский идентификатор транзакции
        /// </summary>
        [JsonPropertyName("trans_id")]
        public int TransID { get; set; }

        /// <summary>
        /// Статус
        /// «0» - транзакция отправлена серверу,
        /// «1» - транзакция получена на сервер QUIK от клиента,
        /// «2» - ошибка при передаче транзакции в торговую систему, поскольку отсутствует подключение шлюза Московской Биржи, повторно транзакция не отправляется,
        /// «3» - транзакция выполнена,
        /// «4» - транзакция не выполнена торговой системой, код ошибки торговой системы будет указан в поле «DESCRIPTION»,
        /// «5» - транзакция не прошла проверку сервера QUIK по каким-либо критериям. Например, проверку на наличие прав у пользователя на отправку транзакции данного типа,
        /// «6» - транзакция не прошла проверку лимитов сервера QUIK,
        /// «10» - транзакция не поддерживается торговой системой. К примеру, попытка отправить «ACTION = MOVE_ORDERS» на Московской Бирже,
        /// «11» - транзакция не прошла проверку правильности электронной подписи. К примеру, если ключи, зарегистрированные на сервере, не соответствуют подписи отправленной транзакции.
        /// «12» - не удалось дождаться ответа на транзакцию, т.к. истек таймаут ожидания. Может возникнуть при подаче транзакций из QPILE.
        /// «13» - транзакция отвергнута, т.к. ее выполнение могло привести к кросс-сделке (т.е. сделке с тем же самым клиентским счетом).
        /// </summary>
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        [JsonPropertyName("result_msg")]
        public string ResultMsg { get; set; }

        /// <summary>
        /// Время (в QLUA представлено как число)
        /// </summary>
        [JsonPropertyName("time")]
        public string Time { get; set; }

        /// <summary>
        /// Идентификатор пользователя у брокера. Для каждого брокера он свой и меняться не должен.
        /// </summary>
        [JsonPropertyName("uid")]
        public double Uid { get; set; }

        /// <summary>
        /// Флаги транзакции (временно не используется)
        /// </summary>
        [JsonPropertyName("flags")]
        public double Flags { get; set; }

        /// <summary>
        /// Идентификатор транзакции на сервере
        /// </summary>
        [JsonPropertyName("server_trans_id")]
        public double ServerTransID { get; set; }

        /// <summary>
        /// Номер заявки
        /// </summary>
        [JsonPropertyName("order_num")]
        public double? OrderNum { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        [JsonPropertyName("price")]
        public double? Price { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        [JsonPropertyName("quantity")]
        public double? Quantity { get; set; }

        /// <summary>
        /// Остаток
        /// </summary>
        [JsonPropertyName("balance")]
        public double? Balance { get; set; }

        /// <summary>
        /// Идентификатор фирмы
        /// </summary>
        [JsonPropertyName("firm_id")]
        public string FirmID { get; set; }

        /// <summary>
        /// Торговый счет
        /// </summary>
        [JsonPropertyName("account")]
        public string Account { get; set; }

        /// <summary>
        /// Код клиента
        /// </summary>
        [JsonPropertyName("client_code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Поручение/комментарий, обычно: код клиента/номер поручения
        /// </summary>
        [JsonPropertyName("brokerref")]
        public string Comment { get; set; }

        /// <summary>
        /// Код класса
        /// </summary>
        [JsonPropertyName("class_code")]
        public string ClassCode { get; set; }

        /// <summary>
        /// Код бумаги
        /// </summary>
        [JsonPropertyName("sec_code")]
        public string SecCode { get; set; }

        /// <summary>
        /// Биржевой номер заявки
        /// </summary>
        [JsonPropertyName("exchange_code")]
        public string ExchangeCode { get; set; }

        /// <summary>
        /// Числовой код ошибки. Значение равно «0», если транзакция выполнена успешно
        /// </summary>
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Источник сообщения. Возможные значения: 
        /// «1» – Торговая система; 
        /// «2» – Сервер QUIK; 
        /// «3» – Библиотека расчёта лимитов; 
        /// «4» – Шлюз торговой системы
        /// </summary>
        [JsonPropertyName("error_source")]
        public int ErrorSource { get; set; }

        /// <summary>
        /// Номер первой заявки, которая выставлялась при автоматической замене кода клиента. Используется, если на сервере QUIK настроена замена кода клиента для кросс-сделки
        /// </summary>
        [JsonPropertyName("first_ordernum")]
        public double FirstOrderNum { get; set; }

        /// <summary>
        /// Дата и время получения шлюзом ответа на транзакцию
        /// </summary>
        [JsonPropertyName("gate_reply_time")]
        public QuikDateTime GateReplyTime { get; set; }

        /// <summary>
        /// Дата и время отправки транзакции, локальное время клиента в UTC
        /// </summary>
        [JsonPropertyName("sent_local_time")]
        public QuikDateTime SentLocalTime { get; set; }

        /// <summary>
        /// Дата и время получения ответа на транзакцию, локальное время клиента в UTC
        /// </summary>
        [JsonPropertyName("got_local_time")]
        public QuikDateTime GotLocalTime { get; set; }

        ///// <summary>
        ///// Заявки. Параметр добавляется в ответ на транзакцию только при наличии двух и более заявок, связанных с транзакцией
        ///// Пока непонятно как реализовывать
        ///// </summary>
        //[JsonPropertyName("orders")]
        //public List<Order> Orders { get; set; }
    }
}