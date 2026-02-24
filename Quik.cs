// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Фасад для работы с QUIK через любой транспорт (TCP / SHM / Mock).
    /// </summary>
    public sealed class Quik : IDisposable
    {
        /// <summary>
        /// Persistent transaction storage
        /// </summary>
        public IPersistentStorage Storage { get; set; }

        /// <summary>
        /// Debug functions
        /// </summary>
        public DebugFunctions Debug { get; set; }

        /// <summary>
        /// Функции обратного вызова
        /// </summary>
        public IQuikEvents Events { get; set; }

        /// <summary>
        /// Сервисные функции
        /// </summary>
        public ServiceFunctions Service { get; private set; }

        /// <summary>
        /// Функции для обращения к спискам доступных параметров
        /// </summary>
        public ClassFunctions Class { get; private set; }

        /// <summary>
        /// Функции для работы со стаканом котировок
        /// </summary>
        public OrderBookFunctions OrderBook { get; set; }

        /// <summary>
        /// Функции взаимодействия скрипта Lua и Рабочего места QUIK
        /// </summary>
        public ITradingFunctions Trading { get; set; }

        /// <summary>
        /// Функции для работы со стоп-заявками
        /// </summary>
        public StopOrderFunctions StopOrders { get; private set; }

        /// <summary>
        /// Функции для работы с заявками
        /// </summary>
        public OrderFunctions Orders { get; private set; }

        /// <summary>
        /// Функции для работы со свечами
        /// </summary>
        public CandleFunctions Candles { get; private set; }

        /// <summary>
        /// Транспорт (TCP, SHM и т.д.)
        /// </summary>
        private readonly IQuikTransport _transport;

        /// <summary>
        /// Конструктор Quik с передачей dqwdqwd
        /// </summary>
        /// <param name="transport">Любой транспорт, реализующий IQuikTransport</param>
        /// <param name="storage">Persistent storage (по умолчанию InMemoryStorage)</param>
        public Quik(IQuikTransport transport, IPersistentStorage storage = null)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            Storage = storage ?? new InMemoryStorage();

            // Создаём адаптер событий
            Events = new QuikEventsAdapter(_transport);

            // Создаём функции QUIK, привязанные к транспорту
            Debug = new DebugFunctions(_transport);
            Service = new ServiceFunctions(_transport);
            Class = new ClassFunctions(_transport);
            OrderBook = new OrderBookFunctions(_transport);
            Trading = new TradingFunctions(_transport);
            StopOrders = new StopOrderFunctions(_transport, this);
            Orders = new OrderFunctions(_transport, this);
            Candles = new CandleFunctions(_transport);
        }

        /// <summary>
        /// Асинхронное подключение к транспортному слою
        /// </summary>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _transport.ConnectAsync(ct);
        }

        /// <summary>
        /// Прекращение работы транспорта
        /// </summary>
        public void StopService()
        {
            _transport.Dispose();
        }

        /// <summary>
        /// Проверка состояния подключения
        /// </summary>
        public bool IsServiceConnected()
        {
            return _transport.IsConnected;
        }

        /// <summary>
        /// Таймаут по умолчанию для отправки запросов
        /// </summary>
        public TimeSpan DefaultSendTimeout
        {
            get => _transport.DefaultSendTimeout;
            set => _transport.DefaultSendTimeout = value;
        }

        /// <summary>
        /// Перевод времени QUIK в UTC
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get; set; }

        /// <summary>
        /// IDisposable
        /// </summary>
        public void Dispose()
        {
            _transport.Dispose();
        }
    }
}