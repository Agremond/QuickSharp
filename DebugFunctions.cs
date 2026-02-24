// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.Transports;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuikSharp
{
    /// <summary>
    /// Отладочные функции QUIK через любой транспорт (TCP, SHM и т.д.)
    /// </summary>
    public class DebugFunctions
    {
        private readonly IQuikTransport _transport;

        public DebugFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        // Пример запроса Ping → Pong
        private class PingRequest : Message<string>
        {
            public PingRequest()
                : base("Ping", "ping", null)
            {
            }
        }

        private class PingResponse : Message<string>
        {
            public PingResponse()
                : base("Pong", "ping", null)
            {
            }
        }

        /// <summary>
        /// Проверка связи с QUIK
        /// </summary>
        public async Task<string> Ping()
        {
            var response = await _transport.SendAsync<PingRequest, PingResponse>(
                new PingRequest(),
                "ping"
            ).ConfigureAwait(false);

            Trace.Assert(response.Data == "Pong");
            return response.Data;
        }

        /// <summary>
        /// Отправка и возврат любого сообщения (echo)
        /// </summary>
        public async Task<T> Echo<T>(T msg)
        {
            var request = new Message<T>(msg, "echo");

            var response = await _transport.SendAsync<Message<T>, Message<T>>(
                request,
                "echo"
            ).ConfigureAwait(false);

            return response.Data;
        }

        /// <summary>
        /// Демонстрация ошибки Lua (divide by zero)
        /// </summary>
        public async Task<string> DivideStringByZero()
        {
            var request = new Message<string>("", "divide_string_by_zero");

            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                request,
                "divide_string_by_zero"
            ).ConfigureAwait(false);

            return response.Data;
        }

        /// <summary>
        /// Проверка, запущено ли приложение внутри QUIK
        /// </summary>
        public async Task<bool> IsQuik()
        {
            var request = new Message<string>("", "is_quik");

            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                request,
                "is_quik"
            ).ConfigureAwait(false);

            return response.Data == "1";
        }
    }
}