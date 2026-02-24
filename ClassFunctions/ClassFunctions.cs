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
            var message = new Message("", "getClassesList");
            var response = await _transport.SendAsync<Message, string>(message, "getClassesList").ConfigureAwait(false);

            return string.IsNullOrEmpty(response)
                ? Array.Empty<string>()
                : response.TrimEnd(',').Split(',');
        }

        public async Task<ClassInfo> GetClassInfo(string classID)
        {
            var message = new Message(classID, "getClassInfo");
            return await _transport.SendAsync<Message, ClassInfo>(message, "getClassInfo").ConfigureAwait(false);
        }

        public async Task<SecurityInfo> GetSecurityInfo(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            var message = new Message(payload, "getSecurityInfo");
            return await _transport.SendAsync<Message, SecurityInfo>(message, "getSecurityInfo").ConfigureAwait(false);
        }

        public async Task<SecurityInfo> GetSecurityInfo(ISecurity security)
        {
            return await GetSecurityInfo(security.ClassCode, security.SecCode).ConfigureAwait(false);
        }

        public async Task<string[]> GetClassSecurities(string classID)
        {
            var message = new Message(classID, "getClassSecurities");
            var response = await _transport.SendAsync<Message, string>(message, "getClassSecurities").ConfigureAwait(false);

            return string.IsNullOrEmpty(response)
                ? Array.Empty<string>()
                : response.TrimEnd(',').Split(',');
        }

        public async Task<string> GetSecurityClass(string classesList, string secCode)
        {
            var payload = $"{classesList}|{secCode}";
            var message = new Message(payload, "getSecurityClass");
            return await _transport.SendAsync<Message, string>(message, "getSecurityClass").ConfigureAwait(false);
        }

        public async Task<string> GetClientCode()
        {
            var message = new Message("", "getClientCode");
            return await _transport.SendAsync<Message, string>(message, "getClientCode").ConfigureAwait(false);
        }

        public async Task<List<string>> GetClientCodes()
        {
            var message = new Message("", "getClientCodes");
            return await _transport.SendAsync<Message, List<string>>(message, "getClientCodes").ConfigureAwait(false);
        }

        public async Task<string> GetTradeAccount(string classCode)
        {
            var message = new Message(classCode, "getTradeAccount");
            return await _transport.SendAsync<Message, string>(message, "getTradeAccount").ConfigureAwait(false);
        }

        public async Task<List<TradesAccounts>> GetTradeAccounts()
        {
            var message = new Message("", "getTradeAccounts");
            return await _transport.SendAsync<Message, List<TradesAccounts>>(message, "getTradeAccounts").ConfigureAwait(false);
        }
    }
}