// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;

namespace QuikSharp
{
    public class TradingFunctions : ITradingFunctions
    {
        private readonly IQuikTransport _transport;

        public TradingFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        // ------------------------------------------------------------------------
        // Депозитарные лимиты
        // ------------------------------------------------------------------------

        public async Task<DepoLimit> GetDepo(string clientCode, string firmId, string secCode, string account)
        {
            var payload = $"{clientCode}|{firmId}|{secCode}|{account}";
            return await _transport.SendAsync<Message, DepoLimit>(
                new Message(payload, "getDepo"), "getDepo").ConfigureAwait(false) ?? new DepoLimit();
        }

        public async Task<DepoLimitEx> GetDepoEx(string firmId, string clientCode, string secCode, string accID, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{secCode}|{accID}|{limitKind}";
            return await _transport.SendAsync<Message, DepoLimitEx>(
                new Message(payload, "getDepoEx"), "getDepoEx").ConfigureAwait(false) ?? new DepoLimitEx();
        }

        public async Task<List<DepoLimitEx>> GetDepoLimits()
        {
            return await _transport.SendAsync<Message, List<DepoLimitEx>>(
                new Message("", "get_depo_limits"), "get_depo_limits").ConfigureAwait(false)
                ?? new List<DepoLimitEx>();
        }

        public async Task<List<DepoLimitEx>> GetDepoLimits(string secCode)
        {
            return await _transport.SendAsync<Message, List<DepoLimitEx>>(
                new Message(secCode, "get_depo_limits"), "get_depo_limits").ConfigureAwait(false)
                ?? new List<DepoLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Денежные лимиты
        // ------------------------------------------------------------------------

        public async Task<MoneyLimit> GetMoney(string clientCode, string firmId, string tag, string currCode)
        {
            var payload = $"{clientCode}|{firmId}|{tag}|{currCode}";
            return await _transport.SendAsync<Message, MoneyLimit>(
                new Message(payload, "getMoney"), "getMoney").ConfigureAwait(false) ?? new MoneyLimit();
        }

        public async Task<MoneyLimitEx> GetMoneyEx(string firmId, string clientCode, string tag, string currCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{tag}|{currCode}|{limitKind}";
            return await _transport.SendAsync<Message, MoneyLimitEx>(
                new Message(payload, "getMoneyEx"), "getMoneyEx").ConfigureAwait(false) ?? new MoneyLimitEx();
        }

        public async Task<List<MoneyLimitEx>> GetMoneyLimits()
        {
            return await _transport.SendAsync<Message, List<MoneyLimitEx>>(
                new Message("", "getMoneyLimits"), "getMoneyLimits").ConfigureAwait(false)
                ?? new List<MoneyLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Фьючерсные лимиты и позиции
        // ------------------------------------------------------------------------

        public async Task<FuturesLimits> GetFuturesLimit(string firmId, string accId, int limitType, string currCode)
        {
            var payload = $"{firmId}|{accId}|{limitType}|{currCode}";
            return await _transport.SendAsync<Message, FuturesLimits>(
                new Message(payload, "getFuturesLimit"), "getFuturesLimit").ConfigureAwait(false) ?? new FuturesLimits();
        }

        public async Task<List<FuturesLimits>> GetFuturesClientLimits()
        {
            return await _transport.SendAsync<Message, List<FuturesLimits>>(
                new Message("", "getFuturesClientLimits"), "getFuturesClientLimits").ConfigureAwait(false)
                ?? new List<FuturesLimits>();
        }

        public async Task<FuturesClientHolding> GetFuturesHolding(string firmId, string accId, string secCode, int posType)
        {
            var payload = $"{firmId}|{accId}|{secCode}|{posType}";
            return await _transport.SendAsync<Message, FuturesClientHolding>(
                new Message(payload, "getFuturesHolding"), "getFuturesHolding").ConfigureAwait(false) ?? new FuturesClientHolding();
        }

        public async Task<List<FuturesClientHolding>> GetFuturesClientHoldings()
        {
            return await _transport.SendAsync<Message, List<FuturesClientHolding>>(
                new Message("", "getFuturesClientHoldings"), "getFuturesClientHoldings").ConfigureAwait(false)
                ?? new List<FuturesClientHolding>();
        }

        // ------------------------------------------------------------------------
        // ParamRequest / Cancel / GetParamEx
        // ------------------------------------------------------------------------

        public async Task<bool> ParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "paramRequest"), "paramRequest").ConfigureAwait(false);
        }

        public Task<bool> ParamRequest(string classCode, string secCode, ParamNames paramName)
            => ParamRequest(classCode, secCode, paramName.ToString());

        public async Task<bool> CancelParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "cancelParamRequest"), "cancelParamRequest").ConfigureAwait(false);
        }

        public Task<bool> CancelParamRequest(string classCode, string secCode, ParamNames paramName)
            => CancelParamRequest(classCode, secCode, paramName.ToString());

        public async Task<ParamTable> GetParamEx(string classCode, string secCode, string paramName, int timeout = Timeout.Infinite)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, ParamTable>(
                new Message(payload, "getParamEx"), "getParamEx").ConfigureAwait(false) ?? new ParamTable();
        }

        public Task<ParamTable> GetParamEx(string classCode, string secCode, ParamNames paramName, int timeout = Timeout.Infinite)
            => GetParamEx(classCode, secCode, paramName.ToString(), timeout);

        public async Task<ParamTable> GetParamEx2(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, ParamTable>(
                new Message(payload, "getParamEx2"), "getParamEx2").ConfigureAwait(false) ?? new ParamTable();
        }

        public Task<ParamTable> GetParamEx2(string classCode, string secCode, ParamNames paramName)
            => GetParamEx2(classCode, secCode, paramName.ToString());

        // ------------------------------------------------------------------------
        // Сделки и все сделки
        // ------------------------------------------------------------------------

        public async Task<List<Trade>> GetTrades()
            => await _transport.SendAsync<Message, List<Trade>>(new Message("", "get_trades"), "get_trades").ConfigureAwait(false) ?? new List<Trade>();

        public async Task<List<Trade>> GetTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            return await _transport.SendAsync<Message, List<Trade>>(new Message(payload, "get_trades"), "get_trades").ConfigureAwait(false) ?? new List<Trade>();
        }

        public async Task<List<Trade>> GetTradesByOrderNumber(long orderNum)
            => await _transport.SendAsync<Message, List<Trade>>(new Message(orderNum.ToString(), "get_Trades_by_OrderNumber"), "get_Trades_by_OrderNumber").ConfigureAwait(false) ?? new List<Trade>();

        public async Task<List<AllTrade>> GetAllTrades()
            => await _transport.SendAsync<Message, List<AllTrade>>(new Message("", "get_all_trades"), "get_all_trades").ConfigureAwait(false) ?? new List<AllTrade>();

        public async Task<List<AllTrade>> GetAllTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            return await _transport.SendAsync<Message, List<AllTrade>>(new Message(payload, "get_all_trades"), "get_all_trades").ConfigureAwait(false) ?? new List<AllTrade>();
        }

        // ------------------------------------------------------------------------
        // Транзакции
        // ------------------------------------------------------------------------

        public async Task<long> SendTransaction(Transaction transaction)
        {
            if (!transaction.TRANS_ID.HasValue || transaction.TRANS_ID.Value == 0)
                throw new ArgumentException("TRANS_ID должен быть установлен перед отправкой");

            if (string.IsNullOrWhiteSpace(transaction.CLIENT_CODE))
                transaction.CLIENT_CODE = transaction.TRANS_ID.Value.ToString();

            try
            {
                var success = await _transport.SendAsync<Message, bool>(
                    new Message(transaction, "sendTransaction"), "sendTransaction").ConfigureAwait(false);

                return success ? transaction.TRANS_ID.Value : -transaction.TRANS_ID.Value;
            }
            catch (Exception ex)
            {
                transaction.ErrorMessage = ex.Message;
                return -transaction.TRANS_ID.Value;
            }
        }

        // ------------------------------------------------------------------------
        // Остальные методы
        // ------------------------------------------------------------------------

        public async Task<DateTime> GetTradeDate()
        {
            var result = await _transport.SendAsync<Message, QuikDateTime>(
                new Message("", "getTradeDate"), "getTradeDate").ConfigureAwait(false);
            return result?.ToDateTime() ?? default;
        }

        public async Task<CalcBuySellResult> CalcBuySell(
            string classCode, string secCode, string clientCode, string trdAccId,
            double price, bool isBuy, bool isMarket)
        {
            var payload = $"{classCode}|{secCode}|{clientCode}|{trdAccId}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}|{isBuy}|{isMarket}";
            return await _transport.SendAsync<Message, CalcBuySellResult>(
                new Message(payload, "calc_buy_sell"), "calc_buy_sell").ConfigureAwait(false) ?? new CalcBuySellResult();
        }

        public async Task<PortfolioInfo> GetPortfolioInfo(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            return await _transport.SendAsync<Message, PortfolioInfo>(
                new Message(payload, "getPortfolioInfo"), "getPortfolioInfo").ConfigureAwait(false) ?? new PortfolioInfo();
        }

        public async Task<PortfolioInfoEx> GetPortfolioInfoEx(string firmId, string clientCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{limitKind}";
            return await _transport.SendAsync<Message, PortfolioInfoEx>(
                new Message(payload, "getPortfolioInfoEx"), "getPortfolioInfoEx").ConfigureAwait(false) ?? new PortfolioInfoEx();
        }

        public async Task<BuySellInfo> GetBuySellInfo(string firmId, string clientCode, string classCode, string secCode, double price)
        {
            var payload = $"{firmId}|{clientCode}|{classCode}|{secCode}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            return await _transport.SendAsync<Message, BuySellInfo>(
                new Message(payload, "getBuySellInfo"), "getBuySellInfo").ConfigureAwait(false) ?? new BuySellInfo();
        }

        public async Task<BuySellInfo> GetBuySellInfoEx(string firmId, string clientCode, string classCode, string secCode, double price)
        {
            var payload = $"{firmId}|{clientCode}|{classCode}|{secCode}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            return await _transport.SendAsync<Message, BuySellInfo>(
                new Message(payload, "getBuySellInfoEx"), "getBuySellInfoEx").ConfigureAwait(false) ?? new BuySellInfo();
        }

        public async Task<string> GetTrdAccByClientCode(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            return await _transport.SendAsync<Message, string>(
                new Message(payload, "GetTrdAccByClientCode"), "GetTrdAccByClientCode").ConfigureAwait(false) ?? string.Empty;
        }

        public async Task<string> GetClientCodeByTrdAcc(string firmId, string trdAccId)
        {
            var payload = $"{firmId}|{trdAccId}";
            return await _transport.SendAsync<Message, string>(
                new Message(payload, "GetClientCodeByTrdAcc"), "GetClientCodeByTrdAcc").ConfigureAwait(false) ?? string.Empty;
        }

        public async Task<bool> IsUcpClient(string firmId, string client)
        {
            var payload = $"{firmId}|{client}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "IsUcpClient"), "IsUcpClient").ConfigureAwait(false);
        }

        public async Task<List<OptionBoard>> GetOptionBoard(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            return await _transport.SendAsync<Message, List<OptionBoard>>(
                new Message(payload, "getOptionBoard"), "getOptionBoard").ConfigureAwait(false) ?? new List<OptionBoard>();
        }
    }
}