// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using System;
using System.Threading.Tasks;
using QuikSharp.DataStructures;
using QuikSharp.Transports;

namespace QuikSharp
{
    /// <summary>
    /// Функции для работы со стаканом котировок через транспорт QUIK
    /// </summary>
    public class OrderBookFunctions
    {
        private readonly IQuikTransport _transport;

        public OrderBookFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<bool> Subscribe(ISecurity security)
        {
            return await Subscribe(security.ClassCode, security.SecCode).ConfigureAwait(false);
        }

        public async Task<bool> Subscribe(string class_code, string sec_code)
        {
            var payload = $"{class_code}|{sec_code}";

            var response = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "Subscribe_Level_II_Quotes"),
                "Subscribe_Level_II_Quotes"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> Unsubscribe(ISecurity security)
        {
            return await Unsubscribe(security.ClassCode, security.SecCode).ConfigureAwait(false);
        }

        public async Task<bool> Unsubscribe(string class_code, string sec_code)
        {
            var payload = $"{class_code}|{sec_code}";

            var response = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "Unsubscribe_Level_II_Quotes"),
                "Unsubscribe_Level_II_Quotes"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<bool> IsSubscribed(ISecurity security)
        {
            return await IsSubscribed(security.ClassCode, security.SecCode).ConfigureAwait(false);
        }

        public async Task<bool> IsSubscribed(string class_code, string sec_code)
        {
            var payload = $"{class_code}|{sec_code}";

            var response = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "IsSubscribed_Level_II_Quotes"),
                "IsSubscribed_Level_II_Quotes"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<OrderBook> GetQuoteLevel2(string class_code, string sec_code)
        {
            var payload = $"{class_code}|{sec_code}";

            var response = await _transport.SendAsync<Message<string>, Message<OrderBook>>(
                new Message<string>(payload, "GetQuoteLevel2"),
                "GetQuoteLevel2"
            ).ConfigureAwait(false);

            return response.Data;
        }
    }
}