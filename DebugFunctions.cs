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

        /// <summary>
        /// Проверка связи с QUIK (Ping → Pong)
        /// </summary>
        public async Task<string> Ping()
        {
            var request = new Message("Ping", "ping");
            var response = await _transport.SendAsync<Message, string>(request, "ping").ConfigureAwait(false);

            Trace.Assert(response == "Pong");
            return response;
        }

        /// <summary>
        /// Отправка и возврат любого сообщения (echo)
        /// </summary>
        public async Task<T> Echo<T>(T msg)
        {
            var request = new Message(msg, "echo");
            var response = await _transport.SendAsync<Message, T>(request, "echo").ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Демонстрация ошибки Lua (divide by zero)
        /// </summary>
        public async Task<string> DivideStringByZero()
        {
            var request = new Message("", "divide_string_by_zero");
            var response = await _transport.SendAsync<Message, string>(request, "divide_string_by_zero").ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Проверка, запущено ли приложение внутри QUIK
        /// </summary>
        public async Task<bool> IsQuik()
        {
            var request = new Message("", "is_quik");
            var response = await _transport.SendAsync<Message, string>(request, "is_quik").ConfigureAwait(false);
            return response == "1";
        }
    }
}