// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp.Transports
{
    /// <summary>
    /// Единый интерфейс для любого транспорта QUIK# (TCP, Shared Memory и потенциально другие).
    /// Определяет контракт для подключения, отправки запросов и получения всех коллбэков.
    /// </summary>
    public interface IQuikTransport : IDisposable
    {
        /// <summary>
        /// Текущее состояние соединения (true = активно)
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Асинхронное подключение к QUIK (TCP — сокеты, SHM — память + семафоры)
        /// </summary>
        Task ConnectAsync(CancellationToken ct = default);

        /// <summary>
        /// Универсальный метод отправки запроса и получения ответа
        /// </summary>
        /// <typeparam name="TRequest">Тип данных запроса (обычно object, анонимный тип или конкретный класс)</typeparam>
        /// <typeparam name="TResponse">Ожидаемый тип данных в поле Data ответа</typeparam>
        /// <param name="request">Данные запроса → попадут в Message.Data</param>
        /// <param name="command">Имя команды (getSecurityInfo, sendTransaction, get_num_candles и т.д.)</param>
        /// <param name="ct">Токен отмены (для таймаутов)</param>
        /// <returns>Типизированный ответ из поля Data</returns>
        Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            string command,
            CancellationToken ct = default);

        // ────────────────────────────────────────────────
        // Все основные события-коллбэки от QUIK (push от Lua)
        // ────────────────────────────────────────────────

        /// <summary>Новая свеча в подписанном таймфрейме</summary>
        event Action<Candle> OnNewCandle;

        /// <summary>Изменение/появление заявки</summary>
        event Action<Order> OnOrder;

        /// <summary>Новая сделка</summary>
        event Action<Trade> OnTrade;

        /// <summary>Ответ на транзакцию (sendTransaction)</summary>
        event Action<TransactionReply> OnTransReply;

        /// <summary>Изменение/появление стоп-заявки</summary>
        event Action<StopOrder> OnStopOrder;

        /// <summary>Обезличенная сделка (лента)</summary>
        event Action<AllTrade> OnAllTrade;

        /// <summary>Изменение стакана котировок (Level II)</summary>
        event Action<OrderBook> OnQuote;

        /// <summary>Изменение параметра инструмента (OnParam)</summary>
        event Action<Param> OnParam;

        /// <summary>Изменение баланса по счёту</summary>
        event Action<AccountBalance> OnAccountBalance;

        /// <summary>Изменение позиции по инструменту</summary>
        event Action<AccountPosition> OnAccountPosition;

        /// <summary>Появление/изменение лимита по бумагам</summary>
        event Action<DepoLimitEx> OnDepoLimit;

        /// <summary>Удаление лимита по бумагам</summary>
        event Action<DepoLimitDelete> OnDepoLimitDelete;

        /// <summary>Новая фирма в справочнике</summary>
        event Action<Firm> OnFirm;

        /// <summary>Изменение позиции по срочному рынку</summary>
        event Action<FuturesClientHolding> OnFuturesClientHolding;

        /// <summary>Изменение лимита по срочному рынку</summary>
        event Action<FuturesLimits> OnFuturesLimitChange;

        /// <summary>Удаление лимита по срочному рынку</summary>
        event Action<FuturesLimitDelete> OnFuturesLimitDelete;

        /// <summary>Изменение денежного лимита клиента</summary>
        event Action<MoneyLimitEx> OnMoneyLimit;

        /// <summary>Удаление денежного лимита</summary>
        event Action<MoneyLimitDelete> OnMoneyLimitDelete;

        /// <summary>Подключение к QUIK установлено (TCP — сокеты открыты, SHM — ресурсы инициализированы)</summary>
        event Action OnConnected;

        /// <summary>Подключение потеряно (сокеты закрыты, SHM недоступен)</summary>
        event Action OnDisconnected;

        // Опциональные / редкие события (можно добавить позже, если понадобятся)

        // event Action<NegDeal> OnNegDeal;
        // event Action<NegTrade> OnNegTrade;
        // event Action OnCleanUp;           // перед выгрузкой скрипта
        // event Action OnClose;             // перед закрытием терминала
    }
}