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
    /// Фасад для работы с QUIK через любой транспорт (TCP / SHM / Mock)
    /// </summary>
    public sealed class Quik : IDisposable
    {
        // ================== Транспорт ==================
        private readonly IQuikTransport _transport;

        // ================== Persistent / Debug ==================
        public IPersistentStorage Storage { get; set; }
        public DebugFunctions Debug { get; private set; }

        // ================== События ==================
        public IQuikEvents Events { get; private set; }

        // ================== Сервисные функции ==================
        public ServiceFunctions Service { get; private set; }
        public ClassFunctions Class { get; private set; }
        public OrderBookFunctions OrderBook { get; private set; }

        // ================== Торговые функции ==================
        public TradingFunctions Trading { get; private set; }
        public StopOrderFunctions StopOrders { get; private set; }
        public OrderFunctions Orders { get; private set; }
        public CandleFunctions Candles { get; private set; }

        // ================== Настройки ==================
        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        // Таймаут по умолчанию для SendAsync, используется если не передан CancellationToken
        private TimeSpan _defaultSendTimeout = TimeSpan.FromSeconds(5);
        public TimeSpan DefaultSendTimeout
        {
            get => _defaultSendTimeout;
            set => _defaultSendTimeout = value;
        }

        /// <summary>
        /// Конструктор Quik с передачей транспорта и опционального PersistentStorage
        /// </summary>
        /// <param name="transport">Любой транспорт, реализующий IQuikTransport</param>
        /// <param name="storage">Persistent storage (по умолчанию InMemoryStorage)</param>
        public Quik(IQuikTransport transport, IPersistentStorage storage = null)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            Storage = storage ?? new InMemoryStorage();

            // ========== События ==========
            Events = new QuikEventsAdapter(_transport);

            // ========== Функции ==========
            Debug = new DebugFunctions(_transport);
            Service = new ServiceFunctions(_transport);
            Class = new ClassFunctions(_transport);
            OrderBook = new OrderBookFunctions(_transport);

            // ========== Торговые функции ==========
            Trading = new TradingFunctions(_transport);
            StopOrders = new StopOrderFunctions(_transport, Trading);
            Orders = new OrderFunctions(_transport);
            Candles = new CandleFunctions(_transport);
        }

        // ================== Подключение / Отключение ==================

        /// <summary>
        /// Асинхронное подключение к транспортному слою
        /// </summary>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await _transport.ConnectAsync(ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Проверка состояния подключения
        /// </summary>
        public bool IsServiceConnected() => _transport.IsConnected;

        /// <summary>
        /// Прекращение работы транспорта
        /// </summary>
        public void StopService()
        {
            _transport.Dispose();
        }

        // ================== Вспомогательные методы ==================

        /// <summary>
        /// Возвращает CancellationToken с дефолтным таймаутом
        /// </summary>
        /// <param name="ct">Приоритетный токен отмены</param>
        /// <returns>CancellationToken с таймаутом</returns>
        public CancellationToken GetCancellationTokenWithDefaultTimeout(CancellationToken ct = default)
        {
            if (ct != default) return ct;

            var cts = new CancellationTokenSource(_defaultSendTimeout);
            return cts.Token;
        }

        // ================== IDisposable ==================
        public void Dispose()
        {
            _transport.Dispose();
        }
    }
}