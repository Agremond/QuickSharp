// Copyright (c) 2026 Your Name / QUIKSharp Community
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using QuikSharp.Transports;
/// Функции взаимодействия скрипта Lua и Рабочего места QUIK
/// +getDepo - функция для получения информации по бумажным лимитам
/// +getMoney - функция для получения информации по денежным лимитам
/// +getMoneyEx - функция для получения информации по денежным лимитам указанного типа
/// +getFuturesLimit - функция для получения информации по фьючерсным лимитам
/// +getFuturesHolding - функция для получения информации по фьючерсным позициям
/// +getFuturesClientHoldings - функция для получения информации по всем фьючерсным позициям
/// +paramRequest - Функция заказывает получение параметров Таблицы текущих торгов
/// +cancelParamRequest - Функция отменяет заказ на получение параметров Таблицы текущих торгов
/// +getParamEx - функция для получения значений Таблицы текущих значений параметров
/// +getParamEx2 - функция для получения всех значений Таблицы текущих значений параметров
/// +getTradeDate - функция для получения даты торговой сессии
/// +sendTransaction - функция для работы с заявками
/// +CalcBuySell - функция для расчета максимально возможного количества лотов в заявке
/// +getPortfolioInfo - функция для получения значений параметров таблицы «Клиентский портфель»
/// +getPortfolioInfoEx - функция для получения значений параметров таблицы «Клиентский портфель» с учетом вида лимита
/// +getBuySellInfo - функция для получения параметров таблицы «Купить/Продать»
/// +getBuySellInfoEx - функция для получения параметров (включая вид лимита) таблицы «Купить/Продать»
/// getTrdAccByClientCode - Функция возвращает торговый счет срочного рынка, соответствующий коду клиента фондового рынка с единой денежной позицией
/// getClientCodeByTrdAcc - Функция возвращает код клиента фондового рынка с единой денежной позицией, соответствующий торговому счету срочного рынка
/// isUcpClient - Функция предназначена для получения признака, указывающего имеет ли клиент единую денежную позицию
namespace QuikSharp
{
    public class TradingFunctions
    {
        private readonly IQuikTransport _transport;

        public TradingFunctions(IQuikTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        // ------------------------------------------------------------------------
        // Депозитарные лимиты
        // ------------------------------------------------------------------------
        /// <summary>
        /// Функция для получения информации по бумажным лимитам
        /// </summary>
        public async Task<DepoLimit> GetDepo(string clientCode, string firmId, string secCode, string account)
        {
            var payload = $"{clientCode}|{firmId}|{secCode}|{account}";
            return await _transport.SendAsync<Message, DepoLimit>(
                new Message(payload, "getDepo"), "getDepo").ConfigureAwait(false) ?? new DepoLimit();
        }
        /// <summary>
        /// Функция для получения информации по бумажным лимитам указанного типа
        /// </summary>
        public async Task<DepoLimitEx> GetDepoEx(string firmId, string clientCode, string secCode, string accID, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{secCode}|{accID}|{limitKind}";
            return await _transport.SendAsync<Message, DepoLimitEx>(
                new Message(payload, "getDepoEx"), "getDepoEx").ConfigureAwait(false) ?? new DepoLimitEx();
        }
        /// <summary>
        /// Возвращает список записей из таблицы 'Лимиты по бумагам'.
        /// </summary>
        public async Task<List<DepoLimitEx>> GetDepoLimits()
        {
            return await _transport.SendAsync<Message, List<DepoLimitEx>>(
                new Message("", "get_depo_limits"), "get_depo_limits").ConfigureAwait(false)
                ?? new List<DepoLimitEx>();
        }
        /// <summary>
        /// Возвращает список записей из таблицы 'Лимиты по бумагам', отфильтрованных по коду инструмента.
        /// </summary>
        /// <param name="secCode">Код инструментаю</param>
        /// <returns></returns>
        public async Task<List<DepoLimitEx>> GetDepoLimits(string secCode)
        {
            return await _transport.SendAsync<Message, List<DepoLimitEx>>(
                new Message(secCode, "get_depo_limits"), "get_depo_limits").ConfigureAwait(false)
                ?? new List<DepoLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Денежные лимиты
        // ------------------------------------------------------------------------
        /// <summary>
        /// Функция для получения информации по денежным лимитам
        /// </summary>
        ///
        public async Task<MoneyLimit> GetMoney(string clientCode, string firmId, string tag, string currCode)
        {
            var payload = $"{clientCode}|{firmId}|{tag}|{currCode}";
            return await _transport.SendAsync<Message, MoneyLimit>(
                new Message(payload, "getMoney"), "getMoney").ConfigureAwait(false) ?? new MoneyLimit();
        }
        /// <summary>
        ///  функция для получения информации по денежным лимитам указанного типа
        /// </summary>
        public async Task<MoneyLimitEx> GetMoneyEx(string firmId, string clientCode, string tag, string currCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{tag}|{currCode}|{limitKind}";
            return await _transport.SendAsync<Message, MoneyLimitEx>(
                new Message(payload, "getMoneyEx"), "getMoneyEx").ConfigureAwait(false) ?? new MoneyLimitEx();
        }
        /// <summary>
        ///  функция для получения информации по денежным лимитам всех торговых счетов (кроме фьючерсных) и валют
        ///  Лучшее место для получения связки clientCode + firmid
        /// </summary>
        public async Task<List<MoneyLimitEx>> GetMoneyLimits()
        {
            return await _transport.SendAsync<Message, List<MoneyLimitEx>>(
                new Message("", "getMoneyLimits"), "getMoneyLimits").ConfigureAwait(false)
                ?? new List<MoneyLimitEx>();
        }

        // ------------------------------------------------------------------------
        // Фьючерсные лимиты и позиции
        // ------------------------------------------------------------------------
        /// <summary>
        ///  функция для получения информации по фьючерсным лимитам
        /// </summary>
        public async Task<FuturesLimits> GetFuturesLimit(string firmId, string accId, int limitType, string currCode)
        {
            var payload = $"{firmId}|{accId}|{limitType}|{currCode}";
            return await _transport.SendAsync<Message, FuturesLimits>(
                new Message(payload, "getFuturesLimit"), "getFuturesLimit").ConfigureAwait(false) ?? new FuturesLimits();
        }
        /// <summary>
        ///  функция для получения информации по фьючерсным лимитам всех клиентских счетов
        /// </summary>
        public async Task<List<FuturesLimits>> GetFuturesClientLimits()
        {
            return await _transport.SendAsync<Message, List<FuturesLimits>>(
                new Message("", "getFuturesClientLimits"), "getFuturesClientLimits").ConfigureAwait(false)
                ?? new List<FuturesLimits>();
        }
        /// <summary>
        ///  функция для получения информации по фьючерсным позициям
        /// </summary>
        public async Task<FuturesClientHolding> GetFuturesHolding(string firmId, string accId, string secCode, int posType)
        {
            var payload = $"{firmId}|{accId}|{secCode}|{posType}";
            return await _transport.SendAsync<Message, FuturesClientHolding>(
                new Message(payload, "getFuturesHolding"), "getFuturesHolding").ConfigureAwait(false) ?? new FuturesClientHolding();
        }
        /// <summary>
        ///  функция для получения информации по всем фьючерсным позициям
        /// </summary>
        public async Task<List<FuturesClientHolding>> GetFuturesClientHoldings()
        {
            return await _transport.SendAsync<Message, List<FuturesClientHolding>>(
                new Message("", "getFuturesClientHoldings"), "getFuturesClientHoldings").ConfigureAwait(false)
                ?? new List<FuturesClientHolding>();
        }

        // ------------------------------------------------------------------------
        // ParamRequest / Cancel / GetParamEx
        // ------------------------------------------------------------------------
        /// <summary>
        /// Функция заказывает получение параметров Таблицы текущих торгов
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public async Task<bool> ParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "paramRequest"), "paramRequest").ConfigureAwait(false);
        }

        public Task<bool> ParamRequest(string classCode, string secCode, ParamNames paramName)
            => ParamRequest(classCode, secCode, paramName.ToString());
        /// <summary>
        /// Функция отменяет заказ на получение параметров Таблицы текущих торгов
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public async Task<bool> CancelParamRequest(string classCode, string secCode, string paramName)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "cancelParamRequest"), "cancelParamRequest").ConfigureAwait(false);
        }

        public Task<bool> CancelParamRequest(string classCode, string secCode, ParamNames paramName)
            => CancelParamRequest(classCode, secCode, paramName.ToString());
        /// <summary>
        /// Функция для получения значений Таблицы текущих значений параметров
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <param name="paramName"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<ParamTable> GetParamEx(string classCode, string secCode, string paramName, int timeout = Timeout.Infinite)
        {
            var payload = $"{classCode}|{secCode}|{paramName}";
            return await _transport.SendAsync<Message, ParamTable>(
                new Message(payload, "getParamEx"), "getParamEx").ConfigureAwait(false) ?? new ParamTable();
        }

        public Task<ParamTable> GetParamEx(string classCode, string secCode, ParamNames paramName, int timeout = Timeout.Infinite)
            => GetParamEx(classCode, secCode, paramName.ToString(), timeout);
        /// <summary>
        /// Функция для получения всех значений Таблицы текущих значений параметров
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
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
        /// <summary>
        /// функция для получения таблицы сделок по заданному инструменту
        /// </summary>
        public async Task<List<Trade>> GetTrades()
            => await _transport.SendAsync<Message, List<Trade>>(new Message("", "get_trades"), "get_trades").ConfigureAwait(false) ?? new List<Trade>();
        /// <summary>
        /// функция для получения таблицы сделок по заданному инструменту
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            return await _transport.SendAsync<Message, List<Trade>>(new Message(payload, "get_trades"), "get_trades").ConfigureAwait(false) ?? new List<Trade>();
        }
        /// <summary>
        /// функция для получения таблицы сделок номеру заявки
        /// </summary>
        /// <param name="orderNum"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesByOrderNumber(long orderNum)
            => await _transport.SendAsync<Message, List<Trade>>(new Message(orderNum.ToString(), "get_Trades_by_OrderNumber"), "get_Trades_by_OrderNumber").ConfigureAwait(false) ?? new List<Trade>();
        /// <summary>
        /// функция для получения таблицы обезличенных сделок
        /// </summary>
        public async Task<List<AllTrade>> GetAllTrades()
            => await _transport.SendAsync<Message, List<AllTrade>>(new Message("", "get_all_trades"), "get_all_trades").ConfigureAwait(false) ?? new List<AllTrade>();
        /// <summary>
        /// функция для получения таблицы обезличенных сделок по заданному инструменту
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <returns></returns>
        public async Task<List<AllTrade>> GetAllTrades(string classCode, string secCode)
        {
            var payload = $"{classCode}|{secCode}";
            return await _transport.SendAsync<Message, List<AllTrade>>(new Message(payload, "get_all_trades"), "get_all_trades").ConfigureAwait(false) ?? new List<AllTrade>();
        }


        // ------------------------------------------------------------------------
        // Остальные методы
        // ------------------------------------------------------------------------
        /// <summary>
        ///  функция для получения даты торговой сессии
        /// </summary>
        public async Task<DateTime> GetTradeDate()
        {
            var result = await _transport.SendAsync<Message, QuikDateTime>(
                new Message("", "getTradeDate"),
                "getTradeDate"
            ).ConfigureAwait(false);

            return result.ToDateTime();
        }
        /// <summary>
        ///  функция для расчета максимально возможного количества лотов в заявке
        ///  При заданном параметре is_market=true, необходимо передать параметр price=0, иначе будет рассчитано максимально возможное количество лотов в заявке по цене price.
        /// </summary>
        public async Task<CalcBuySellResult> CalcBuySell(
            string classCode, string secCode, string clientCode, string trdAccId,
            double price, bool isBuy, bool isMarket)
        {
            var payload = $"{classCode}|{secCode}|{clientCode}|{trdAccId}|{price.ToString(System.Globalization.CultureInfo.InvariantCulture)}|{isBuy}|{isMarket}";
            return await _transport.SendAsync<Message, CalcBuySellResult>(
                new Message(payload, "calc_buy_sell"), "calc_buy_sell").ConfigureAwait(false) ?? new CalcBuySellResult();
        }
        /// <summary>
        ///  функция для получения значений параметров таблицы «Клиентский портфель»
        /// </summary>
        public async Task<PortfolioInfo> GetPortfolioInfo(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            return await _transport.SendAsync<Message, PortfolioInfo>(
                new Message(payload, "getPortfolioInfo"), "getPortfolioInfo").ConfigureAwait(false) ?? new PortfolioInfo();
        }
        /// <summary>
        ///  функция для получения значений параметров таблицы «Клиентский портфель» с учетом вида лимита
        ///  Для получения значений параметров таблицы «Клиентский портфель» для клиентов срочного рынка без единой денежной позиции
        ///  необходимо указать в качестве «clientCode» – торговый счет на срочном рынке, а в качестве «limitKind» – 0.
        /// </summary>
        public async Task<PortfolioInfoEx> GetPortfolioInfoEx(string firmId, string clientCode, int limitKind)
        {
            var payload = $"{firmId}|{clientCode}|{limitKind}";
            return await _transport.SendAsync<Message, PortfolioInfoEx>(
                new Message(payload, "getPortfolioInfoEx"), "getPortfolioInfoEx").ConfigureAwait(false) ?? new PortfolioInfoEx();
        }
        /// <summary>
        ///  функция для получения параметров таблицы «Купить/Продать»
        /// </summary>
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
        /// <summary>
        /// Функция возвращает торговый счет срочного рынка, соответствующий коду клиента фондового рынка с единой денежной позицией
        /// </summary>
        /// <param name="firmId"></param>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        public async Task<string> GetTrdAccByClientCode(string firmId, string clientCode)
        {
            var payload = $"{firmId}|{clientCode}";
            return await _transport.SendAsync<Message, string>(
                new Message(payload, "GetTrdAccByClientCode"), "GetTrdAccByClientCode").ConfigureAwait(false) ?? string.Empty;
        }
        /// <summary>
        /// Функция возвращает код клиента фондового рынка с единой денежной позицией, соответствующий торговому счету срочного рынка
        /// </summary>
        /// <param name="firmId"></param>
        /// <param name="trdAccId"></param>
        /// <returns></returns>
        public async Task<string> GetClientCodeByTrdAcc(string firmId, string trdAccId)
        {
            var payload = $"{firmId}|{trdAccId}";
            return await _transport.SendAsync<Message, string>(
                new Message(payload, "GetClientCodeByTrdAcc"), "GetClientCodeByTrdAcc").ConfigureAwait(false) ?? string.Empty;
        }
        /// <summary>
        /// Функция предназначена для получения признака, указывающего имеет ли клиент единую денежную позицию
        /// </summary>
        /// <param name="firmId">идентификатор фирмы фондового рынка</param>
        /// <param name="client">код клиента фондового рынка или торговый счет срочного рынка</param>
        /// <returns></returns>
        public async Task<bool> IsUcpClient(string firmId, string client)
        {
            var payload = $"{firmId}|{client}";
            return await _transport.SendAsync<Message, bool>(
                new Message(payload, "IsUcpClient"), "IsUcpClient").ConfigureAwait(false);
        }
        /// <summary>
        /// Функция получения доски опционов
        /// </summary>
        /// <param name="classCode"></param>
        /// <param name="secCode"></param>
        /// <returns></returns>
        public async Task<List<OptionBoard>> GetOptionBoard(string classCode, string secCode, string series)
        {
            var payload = $"{classCode}|{secCode}|{series}";
            return await _transport.SendAsync<Message, List<OptionBoard>>(
                new Message(payload, "getOptionBoard"), "getOptionBoard").ConfigureAwait(false) ?? new List<OptionBoard>();
        }

    }
}