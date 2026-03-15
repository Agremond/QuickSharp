// Copyright (c) 2026 Your Name / QuikSharp Community
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
        /// Функция предназначена для получения информации о свечках по идентификатору (заказ данных для построения графика плагин не осуществляет, поэтому для успешного доступа нужный график должен быть открыт).
        /// </summary>
        /// <param name="graphicTag">Строковый идентификатор графика или индикатора</param>
        /// <param name="line">Номер линии графика или индикатора. Первая линия имеет номер 0</param>
        /// <param name="first">Индекс первой свечки. Первая (самая левая) свечка имеет индекс 0</param>
        /// <param name="count">Количество запрашиваемых свечек</param>
        /// <returns></returns>
        public async Task<List<Candle>> GetCandles(string graphicTag, int line, int first, int count)
        {
            var payload = $"{graphicTag}|{line}|{first}|{count}";
            var message = new Message(payload, "get_candles");
            return await _transport.SendAsync<Message, List<Candle>>(message, "get_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Функция возвращает список свечек указанного инструмента заданного интервала и параметра запрошенных данных.
        /// </summary>
        /// <param name="classCode">Класс инструмента.</param>
        /// <param name="securityCode">Код инструмента.</param>
        /// <param name="interval">Интервал свечей.</param>
        /// <param name="param">Параметр запрашиваемых свечей.</param>
        /// <returns>Список свечей.</returns>
        public async Task<List<Candle>> GetAllCandles(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            return await GetLastCandles(classCode, securityCode, interval, 0, param).ConfigureAwait(false);
        }
        /// <summary>
        /// Осуществляет подписку на получение исторических данных (свечи)
        /// </summary>
        /// <param name="classCode">Класс инструмента.</param>
        /// <param name="securityCode">Код инструмента.</param>
        /// <param name="interval">интервал свечей (тайм-фрейм).</param>
        /// <param name="param">Параметр запрашиваемых свечей.</param>
        public async Task Subscribe(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "subscribe_to_candles");
            await _transport.SendAsync<Message, string>(message, "subscribe_to_candles").ConfigureAwait(false);
        }


        /// <summary>
        /// Возвращает заданное количество свечек указанного инструмента и интервала с конца.
        /// </summary>
        /// <param name="classCode">Класс инструмента.</param>
        /// <param name="securityCode">Код инструмента.</param>
        /// <param name="interval">Интервал свечей.</param>
        /// <param name="param">Параметр запрашиваемых свечей.</param>
        /// <param name="count">Количество возвращаемых свечей с конца.</param>
        /// <returns>Список свечей.</returns>
        public async Task<List<Candle>> GetLastCandles(string classCode, string securityCode, CandleInterval interval, int count, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}|{count}";
            var message = new Message(payload, "get_candles_from_data_source");
            return await _transport.SendAsync<Message, List<Candle>>(message, "get_candles_from_data_source").ConfigureAwait(false);
        }

  

        /// <summary>
        /// Отписывается от получения исторических данных (свечей)
        /// </summary>
        /// <param name="classCode">Класс инструмента.</param>
        /// <param name="securityCode">Код инструмента.</param>
        /// <param name="interval">интервал свечей (тайм-фрейм).</param>
        /// <param name="param">Параметр запрашиваемых свечей.</param>
        public async Task Unsubscribe(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "unsubscribe_from_candles");
            await _transport.SendAsync<Message, string>(message, "unsubscribe_from_candles").ConfigureAwait(false);
        }

        /// <summary>
        /// Проверка состояния подписки на исторические данные (свечи)
        /// </summary>
        /// <param name="classCode">Класс инструмента.</param>
        /// <param name="securityCode">Код инструмента.</param>
        /// <param name="interval">интервал свечей (тайм-фрейм).</param>
        /// <param name="param">Параметр запрашиваемых свечей.</param>
        public async Task<bool> IsSubscribed(string classCode, string securityCode, CandleInterval interval, string param = "-")
        {
            var payload = $"{classCode}|{securityCode}|{(int)interval}|{param}";
            var message = new Message(payload, "is_subscribed");
            return await _transport.SendAsync<Message, bool>(message, "is_subscribed").ConfigureAwait(false);
        }
    }
}