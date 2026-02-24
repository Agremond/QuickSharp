// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace QuikSharp
{
    public interface ITradingFunctions
    {
        Task<DepoLimit> GetDepo(string clientCode, string firmId, string secCode, string account);
        Task<DepoLimitEx> GetDepoEx(string firmId, string clientCode, string secCode, string accID, int limitKind);
        Task<List<DepoLimitEx>> GetDepoLimits();
        Task<List<DepoLimitEx>> GetDepoLimits(string secCode);

        Task<MoneyLimit> GetMoney(string clientCode, string firmId, string tag, string currCode);
        Task<MoneyLimitEx> GetMoneyEx(string firmId, string clientCode, string tag, string currCode, int limitKind);
        Task<List<MoneyLimitEx>> GetMoneyLimits();

        Task<FuturesLimits> GetFuturesLimit(string firmId, string accId, int limitType, string currCode);
        Task<List<FuturesLimits>> GetFuturesClientLimits();

        Task<FuturesClientHolding> GetFuturesHolding(string firmId, string accId, string secCode, int posType);
        Task<List<FuturesClientHolding>> GetFuturesClientHoldings();

        Task<List<OptionBoard>> GetOptionBoard(string classCode, string secCode);

        Task<bool> ParamRequest(string classCode, string secCode, string paramName);
        Task<bool> ParamRequest(string classCode, string secCode, ParamNames paramName);

        Task<bool> CancelParamRequest(string classCode, string secCode, string paramName);
        Task<bool> CancelParamRequest(string classCode, string secCode, ParamNames paramName);

        Task<ParamTable> GetParamEx(string classCode, string secCode, string paramName, int timeout = Timeout.Infinite);
        Task<ParamTable> GetParamEx(string classCode, string secCode, ParamNames paramName, int timeout = Timeout.Infinite);

        Task<ParamTable> GetParamEx2(string classCode, string secCode, string paramName);
        Task<ParamTable> GetParamEx2(string classCode, string secCode, ParamNames paramName);

        Task<List<Trade>> GetTrades();
        Task<List<Trade>> GetTrades(string classCode, string secCode);
        Task<List<Trade>> GetTradesByOrderNumber(long orderNum);

        Task<DateTime> GetTradeDate();

        Task<long> SendTransaction(Transaction transaction);

        Task<CalcBuySellResult> CalcBuySell(string classCode, string secCode, string clientCode, string trdAccId, double price, bool isBuy, bool isMarket);

        Task<PortfolioInfo> GetPortfolioInfo(string firmId, string clientCode);
        Task<PortfolioInfoEx> GetPortfolioInfoEx(string firmId, string clientCode, int limitKind);

        Task<BuySellInfo> GetBuySellInfo(string firmId, string clientCode, string classCode, string secCode, double price);
        Task<BuySellInfo> GetBuySellInfoEx(string firmId, string clientCode, string classCode, string secCode, double price);

        Task<string> GetTrdAccByClientCode(string firmId, string clientCode);
        Task<string> GetClientCodeByTrdAcc(string firmId, string trdAccId);

        Task<bool> IsUcpClient(string firmId, string client);

        Task<List<AllTrade>> GetAllTrades();
        Task<List<AllTrade>> GetAllTrades(string classCode, string secCode);
    }

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
            var resp = await _transport.SendAsync<Message<string>, Message<DepoLimit>>(
                new Message<string>(payload, "getDepo"), "getDepo");
            return resp.Data ?? new DepoLimit();
        }

        public async Task<DepoLimitEx> GetDepoEx(string firmId, string clientCode, string secCode, string accID, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{secCode}|{accID}|{limitKind}";
            var resp = await _transport.SendAsync<Message<string>, Message<DepoLimitEx>>(
                new Message<string>(payload, "getDepoEx"), "getDepoEx");
            return resp.Data ?? new DepoLimitEx();
        }

        public async Task<List<DepoLimitEx>> GetDepoLimits()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<DepoLimitEx>>>(
                new Message<string>("", "get_depo_limits"), "get_depo_limits");
            return resp.Data ?? new List<DepoLimitEx>();
        }

        public async Task<List<DepoLimitEx>> GetDepoLimits(string secCode)
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<DepoLimitEx>>>(
                new Message<string>(secCode, "get_depo_limits"), "get_depo_limits");
            return resp.Data ?? new List<DepoLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Денежные лимиты
        // ------------------------------------------------------------------------

        public async Task<MoneyLimit> GetMoney(string clientCode, string firmId, string tag, string currCode)
        {
            var payload = $"{clientCode}|{firmId}|{tag}|{currCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<MoneyLimit>>(
                new Message<string>(payload, "getMoney"), "getMoney");
            return resp.Data ?? new MoneyLimit();
        }

        public async Task<MoneyLimitEx> GetMoneyEx(string firmId, string clientCode, string tag, string currCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{tag}|{currCode}|{limitKind}";
            var resp = await _transport.SendAsync<Message<string>, Message<MoneyLimitEx>>(
                new Message<string>(payload, "getMoneyEx"), "getMoneyEx");
            return resp.Data ?? new MoneyLimitEx();
        }

        public async Task<List<MoneyLimitEx>> GetMoneyLimits()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<MoneyLimitEx>>>(
                new Message<string>("", "getMoneyLimits"), "getMoneyLimits");
            return resp.Data ?? new List<MoneyLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Фьючерсные лимиты и позиции
        // ------------------------------------------------------------------------

        public async Task<FuturesLimits> GetFuturesLimit(string firmId, string accId, int limitType, string currCode)
        {
            var payload = $"{firmId}|{accId}|{limitType}|{currCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<FuturesLimits>>(
                new Message<string>(payload, "getFuturesLimit"), "getFuturesLimit");
            return resp.Data ?? new FuturesLimits();
        }

        public async Task<List<FuturesLimits>> GetFuturesClientLimits()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<FuturesLimits>>>(
                new Message<string>("", "getFuturesClientLimits"), "getFuturesClientLimits");
            return resp.Data ?? new List<FuturesLimits>();
        }

        public async Task<FuturesClientHolding> GetFuturesHolding(string firmId, string accId, string secCode, int posType)
        {
            var payload = $"{firmId}|{accId}|{secCode}|{posType}";
            var resp = await _transport.SendAsync<Message<string>, Message<FuturesClientHolding>>(
                new Message<string>(payload, "getFuturesHolding"), "getFuturesHolding");
            return resp.Data ?? new FuturesClientHolding();
        }

        public async Task<List<FuturesClientHolding>> GetFuturesClientHoldings()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<FuturesClientHolding>>>(
                new Message<string>("", "getFuturesClientHoldings"), "getFuturesClientHoldings");
            return resp.Data ?? new List<FuturesClientHolding>();
        }

        // ------------------------------------------------------------------------
        // ParamRequest / Cancel / GetParamEx
        // ------------------------------------------------------------------------

        public async Task<bool> ParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            var resp = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "paramRequest"), "paramRequest");
            return resp.Data;
        }

        public Task<bool> ParamRequest(string classCode, string secCode, ParamNames paramName)
            => ParamRequest(classCode, secCode, paramName.ToString());

        public async Task<bool> CancelParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            var resp = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "cancelParamRequest"), "cancelParamRequest");
            return resp.Data;
        }

        public Task<bool> CancelParamRequest(string classCode, string secCode, ParamNames paramName)
            => CancelParamRequest(classCode, secCode, paramName.ToString());

        public async Task<ParamTable> GetParamEx(string classCode, string secCode, string paramName, int timeout = Timeout.Infinite)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            var resp = await _transport.SendAsync<Message<string>, Message<ParamTable>>(
                new Message<string>(payload, "getParamEx"), "getParamEx");
            return resp.Data ?? new ParamTable();
        }

        public Task<ParamTable> GetParamEx(string classCode, string secCode, ParamNames paramName, int timeout = Timeout.Infinite)
            => GetParamEx(classCode, secCode, paramName.ToString(), timeout);

        public async Task<ParamTable> GetParamEx2(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            var resp = await _transport.SendAsync<Message<string>, Message<ParamTable>>(
                new Message<string>(payload, "getParamEx2"), "getParamEx2");
            return resp.Data ?? new ParamTable();
        }

        public Task<ParamTable> GetParamEx2(string classCode, string secCode, ParamNames paramName)
            => GetParamEx2(classCode, secCode, paramName.ToString());

        // ------------------------------------------------------------------------
        // Сделки
        // ------------------------------------------------------------------------

        public async Task<List<Trade>> GetTrades()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<Trade>>>(
                new Message<string>("", "get_trades"), "get_trades");
            return resp.Data ?? new List<Trade>();
        }

        public async Task<List<Trade>> GetTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<List<Trade>>>(
                new Message<string>(payload, "get_trades"), "get_trades");
            return resp.Data ?? new List<Trade>();
        }

        public async Task<List<Trade>> GetTradesByOrderNumber(long orderNum)
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<Trade>>>(
                new Message<string>(orderNum.ToString(), "get_Trades_by_OrderNumber"), "get_Trades_by_OrderNumber");
            return resp.Data ?? new List<Trade>();
        }

        public async Task<List<AllTrade>> GetAllTrades()
        {
            var resp = await _transport.SendAsync<Message<string>, Message<List<AllTrade>>>(
                new Message<string>("", "get_all_trades"), "get_all_trades");
            return resp.Data ?? new List<AllTrade>();
        }

        public async Task<List<AllTrade>> GetAllTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<List<AllTrade>>>(
                new Message<string>(payload, "get_all_trades"), "get_all_trades");
            return resp.Data ?? new List<AllTrade>();
        }

        // ------------------------------------------------------------------------
        // Транзакции
        // ------------------------------------------------------------------------

        public async Task<long> SendTransaction(Transaction transaction)
        {
            if (!transaction.TRANS_ID.HasValue || transaction.TRANS_ID.Value == 0)
            {
                throw new ArgumentException("TRANS_ID должен быть установлен перед отправкой");
            }

            if (string.IsNullOrWhiteSpace(transaction.CLIENT_CODE))
            {
                transaction.CLIENT_CODE = transaction.TRANS_ID.Value.ToString();
            }

            try
            {
                var resp = await _transport.SendAsync<Message<Transaction>, Message<bool>>(
                    new Message<Transaction>(transaction, "sendTransaction"),
                    "sendTransaction");

                if (resp.Data)
                {
                    return transaction.TRANS_ID.Value;
                }

                return -transaction.TRANS_ID.Value;
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
            var resp = await _transport.SendAsync<Message<string>, Message<QuikDateTime>>(
                new Message<string>("", "getTradeDate"), "getTradeDate");

            return resp.Data?.ToDateTime() ?? default;
        }

        public async Task<CalcBuySellResult> CalcBuySell(
            string classCode, string secCode, string clientCode, string trdAccId,
            double price, bool isBuy, bool isMarket)
        {
            var quikPrice = price.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var payload = $"{classCode}|{secCode}|{clientCode}|{trdAccId}|{quikPrice}|{isBuy}|{isMarket}";

            var resp = await _transport.SendAsync<Message<string>, Message<CalcBuySellResult>>(
                new Message<string>(payload, "calc_buy_sell"), "calc_buy_sell");

            return resp.Data ?? new CalcBuySellResult();
        }

        public async Task<PortfolioInfo> GetPortfolioInfo(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<PortfolioInfo>>(
                new Message<string>(payload, "getPortfolioInfo"), "getPortfolioInfo");
            return resp.Data ?? new PortfolioInfo();
        }

        public async Task<PortfolioInfoEx> GetPortfolioInfoEx(string firmId, string clientCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{limitKind}";
            var resp = await _transport.SendAsync<Message<string>, Message<PortfolioInfoEx>>(
                new Message<string>(payload, "getPortfolioInfoEx"), "getPortfolioInfoEx");
            return resp.Data ?? new PortfolioInfoEx();
        }

        public async Task<BuySellInfo> GetBuySellInfo(string firmId, string clientCode, string classCode, string secCode, double price)
        {
            var payload = $"{firmId}|{clientCode}|{classCode}|{secCode}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            var resp = await _transport.SendAsync<Message<string>, Message<BuySellInfo>>(
                new Message<string>(payload, "getBuySellInfo"), "getBuySellInfo");
            return resp.Data ?? new BuySellInfo();
        }

        public async Task<BuySellInfo> GetBuySellInfoEx(string firmId, string clientCode, string classCode, string secCode, double price)
        {
            var payload = $"{firmId}|{clientCode}|{classCode}|{secCode}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            var resp = await _transport.SendAsync<Message<string>, Message<BuySellInfo>>(
                new Message<string>(payload, "getBuySellInfoEx"), "getBuySellInfoEx");
            return resp.Data ?? new BuySellInfo();
        }

        public async Task<string> GetTrdAccByClientCode(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(payload, "GetTrdAccByClientCode"), "GetTrdAccByClientCode");
            return resp.Data ?? string.Empty;
        }

        public async Task<string> GetClientCodeByTrdAcc(string firmId, string trdAccId)
        {
            var payload = $"{firmId}|{trdAccId}";
            var resp = await _transport.SendAsync<Message<string>, Message<string>>(
                new Message<string>(payload, "GetClientCodeByTrdAcc"), "GetClientCodeByTrdAcc");
            return resp.Data ?? string.Empty;
        }

        public async Task<bool> IsUcpClient(string firmId, string client)
        {
            var payload = $"{firmId}|{client}";
            var resp = await _transport.SendAsync<Message<string>, Message<bool>>(
                new Message<string>(payload, "IsUcpClient"), "IsUcpClient");
            return resp.Data;
        }

        public async Task<List<OptionBoard>> GetOptionBoard(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            var resp = await _transport.SendAsync<Message<string>, Message<List<OptionBoard>>>(
                new Message<string>(payload, "getOptionBoard"), "getOptionBoard");
            return resp.Data ?? new List<OptionBoard>();
        }
    }
}