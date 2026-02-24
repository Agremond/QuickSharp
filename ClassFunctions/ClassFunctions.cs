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
    /// Функции для обращения к спискам доступных параметров через транспорт QUIK
    /// </summary>
    public class ClassFunctions
    {
        private readonly IQuikTransport _transport;

        public ClassFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<string[]> GetClassesList()
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>("", "getClassesList"),
                "getClassesList"
            ).ConfigureAwait(false);

            return string.IsNullOrEmpty(response.Data)
                ? Array.Empty<string>()
                : response.Data.TrimEnd(',').Split(',');
        }

        public async Task<ClassInfo> GetClassInfo(string classID)
        {
            var response = await _transport.SendAsync<Message<string>, Message<ClassInfo>>(
                new Message<string>(classID, "getClassInfo"),
                "getClassInfo"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<SecurityInfo> GetSecurityInfo(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";

            var response = await _transport.SendAsync<Message<string>, Message<SecurityInfo>>(
                new Message<string>(payload, "getSecurityInfo"),
                "getSecurityInfo"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<SecurityInfo> GetSecurityInfo(ISecurity security)
        {
            return await GetSecurityInfo(security.ClassCode, security.SecCode).ConfigureAwait(false);
        }

        public async Task<string[]> GetClassSecurities(string classID)
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(classID, "getClassSecurities"),
                "getClassSecurities"
            ).ConfigureAwait(false);

            return string.IsNullOrEmpty(response.Data)
                ? Array.Empty<string>()
                : response.Data.TrimEnd(',').Split(',');
        }

        public async Task<string> GetSecurityClass(string classesList, string secCode)
        {
            var payload = $"{classesList}|{secCode}";

            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(payload, "getSecurityClass"),
                "getSecurityClass"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<string> GetClientCode()
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>("", "getClientCode"),
                "getClientCode"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<List<string>> GetClientCodes()
        {
            var response = await _transport.SendAsync<Message<string>, Message<List<string>>>(
                new Message<string>("", "getClientCodes"),
                "getClientCodes"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<string> GetTradeAccount(string classCode)
        {
            var response = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(classCode, "getTradeAccount"),
                "getTradeAccount"
            ).ConfigureAwait(false);

            return response.Data;
        }

        public async Task<List<TradesAccounts>> GetTradeAccounts()
        {
            var response = await _transport.SendAsync<Message<string>, Message<List<TradesAccounts>>>(
                new Message<string>("", "getTradeAccounts"),
                "getTradeAccounts"
            ).ConfigureAwait(false);

            return response.Data;
        }
    }
}