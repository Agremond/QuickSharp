// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.Transports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Функции для получения свечей через любой IQuikTransport (TCP, SHM)
    /// </summary>
    public class CandleFunctions
    {
        private readonly IQuikTransport _transport;

        /// <summary>
        /// Событие получения новой свечи
        /// </summary>
        public event Action<Candle>? NewCandle;

        /// <summary>
        /// Конструктор с любым транспортом QUIK#
        /// </summary>
        /// <param name="transport">Реализация IQuikTransport (например, ShmQuikTransport)</param>
        public CandleFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _transport.OnNewCandle += RaiseNewCandleEvent;
        }

        private void RaiseNewCandleEvent(Candle candle)
        {
            NewCandle?.Invoke(candle);
        }

        /// <summary>
        /// Получение количества свечей по графическому тегу
        /// </summary>
        public async Task<int> GetNumCandles(string graphicTag)
        {
            var message = new Message(graphicTag, "get_num_candles");
            return await _transport.SendAsync<Message, int>(message, "get_num_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Получение всех свечей по графическому тегу
        /// </summary>
        public async Task<List<Candle>> GetAllCandles(string graphicTag)
        {
            return await GetCandles(graphicTag, 0, 0, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// Получение свечей по графическому тегу с указанной линией и диапазоном
        /// </summary>
        public async Task<List<Candle>> GetCandles(string graphicTag, int line, int first, int count)
        {
            var payload = $"{graphicTag}|{line}|{first}|{count}";
            var message = new Message(payload, "get_candles");
            return await _transport.SendAsync<Message, List<Candle>>(message, "get_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Получение всех свечей инструмента по таймфрейму
        /// </summary>
        public async Task<List<Candle>> GetAllCandles(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            return await GetLastCandles(classCode, securityCode, interval, 0, param).ConfigureAwait(false);
        }

        /// <summary>
        /// Получение последних N свечей инструмента по таймфрейму
        /// </summary>
        public async Task<List<Candle>> GetLastCandles(string classCode, string securityCode, CandleInterval interval, int count, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}|{count}";
            var message = new Message(payload, "get_candles_from_data_source");
            return await _transport.SendAsync<Message, List<Candle>>(message, "get_candles_from_data_source").ConfigureAwait(false);
        }

        /// <summary>
        /// Подписка на свечи инструмента
        /// </summary>
        public async Task Subscribe(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "subscribe_to_candles");
            await _transport.SendAsync<Message, string>(message, "subscribe_to_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Отписка от свечей инструмента
        /// </summary>
        public async Task Unsubscribe(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "unsubscribe_from_candles");
            await _transport.SendAsync<Message, string>(message, "unsubscribe_from_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Проверка состояния подписки на свечи инструмента
        /// </summary>
        public async Task<bool> IsSubscribed(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "is_subscribed");
            return await _transport.SendAsync<Message, bool>(message, "is_subscribed").ConfigureAwait(false);
        }
    }
}