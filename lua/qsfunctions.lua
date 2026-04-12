-- qsfunctions.lua
-- Обработчики команд QUIK# (Lua ? C# / внешний процесс)
-- Вызываются через dispatch_and_process при получении JSON-сообщения с полем "cmd"

local json = require "dkjson"

local qsfunctions = {}
local is_debug = false

local function send_error(msg, error_text)
    msg.cmd       = "lua_error"
    msg.lua_error = tostring(error_text)
    msg.data      = nil
    return msg
end
--------------------------------------------------------------------------------
-- Основной диспетчер команд
--------------------------------------------------------------------------------

--- Выполняет команду по msg.cmd или возвращает ошибку
-- Вспомогательная функция для получения traceback
local function get_traceback(err)
    return debug.traceback("Lua error: " .. tostring(err), 2)  -- уровень 2, чтобы пропустить сам xpcall
end
function qsfunctions.dispatch_and_process(msg)
    if type(msg) ~= "table" then
        log("dispatch_and_process: msg is not a table", 1)
        return { cmd = "lua_error", lua_error = "Некорректный формат сообщения (не таблица)" }
    end

    if not msg.cmd or type(msg.cmd) ~= "string" then
        log("dispatch_and_process: msg.cmd missing or not string", 1)
        return { cmd = "lua_error", lua_error = "Некорректный формат сообщения (нет cmd)" }
    end

    local handler = qsfunctions[msg.cmd]
    if not handler then
        log("Неизвестная команда: " .. tostring(msg.cmd), 3)
        return send_error(msg, "Command not implemented: " .. tostring(msg.cmd))
    end

    -- Выполняем обработчик с полным traceback
    local status, result = xpcall(
        function() return handler(msg) end,
        get_traceback
    )
    
   
    if status then
        local res = result or msg

        if type(res.lua_error) == "string" and res.lua_error ~= "" then
            res.cmd = "lua_error"
            --log("lua_error DD : " .. type(result) .. "   " .. res.lua_error, 3)
            res.data = json.null
        else
            res.cmd = res.cmd or msg.cmd
            res.lua_error = nil
            if res.data == nil then
                res.data = json.null        -- или {} — но null лучше для различия "нет данных" и "ошибка"
            end
        end

        res.req_id = msg.req_id             -- на всякий случай
        return res
    end
end

--------------------------------------------------------------------------------
-- Отладочные и тестовые команды
--------------------------------------------------------------------------------

function qsfunctions.ping(msg)
    msg.t = timemsec()
    local inner = msg.data
    if type(inner) == "table" and inner.data == "Ping" then
        msg.data = "Pong"
    elseif inner == "Ping" then
        msg.data = "Pong"
    else
        msg.data = tostring(inner) .. " ? Ping"
    end
    return msg
end

function qsfunctions.echo(msg)
    msg.t = timemsec()
    return msg
end

function qsfunctions.is_quik(msg)
    msg.t = timemsec()
    msg.data = getScriptPath and 1 or 0
    return msg
end

function qsfunctions.divide_string_by_zero(msg)
    msg.data = "asd" / 0     -- для теста обработки ошибок
    return msg
end

--------------------------------------------------------------------------------
-- Сервисные функции QUIK
--------------------------------------------------------------------------------

function qsfunctions.isConnected(msg)
    msg.t = timemsec()
    msg.data = tostring(isConnected())
    return msg
end

function qsfunctions.getWorkingFolder(msg)
    msg.t = timemsec()
    msg.data = getWorkingFolder()
    return msg
end

function qsfunctions.getScriptPath(msg)
    msg.t = timemsec()
    msg.data = getScriptPath()
    return msg
end

function qsfunctions.getInfoParam(msg)
    msg.t = timemsec()
    msg.data = getInfoParam(msg.data.data)
    return msg
end

function qsfunctions.message(msg)
    log(msg.data or "", 1)
    msg.data = ""
    return msg
end

function qsfunctions.warning_message(msg)
    log(msg.data or "", 2)
    msg.data = ""
    return msg
end

function qsfunctions.error_message(msg)
    log(msg.data or "", 3)
    msg.data = ""
    return msg
end

function qsfunctions.sleep(msg)
    delay(tonumber(msg.data) or 0)
    msg.data = ""
    return msg
end

function qsfunctions.PrintDbgStr(msg)
    log(msg.data or "", 0)
    msg.data = ""
    return msg
end

--------------------------------------------------------------------------------
-- Метки (Labels) на графике
--------------------------------------------------------------------------------

function qsfunctions.addLabel(msg)
    local spl = split(msg.data, "|")
    local price, curdate, curtime, qty, path, id, algmnt, bgnd =
        spl[1], spl[2], spl[3], spl[4], spl[5], spl[6], spl[7], spl[8]

    local label = {
        TEXT                   = "",
        IMAGE_PATH             = path,
        ALIGNMENT              = algmnt,
        YVALUE                 = tostring(price),
        DATE                   = tostring(curdate),
        TIME                   = tostring(curtime),
        R                      = 255,
        G                      = 255,
        B                      = 255,
        TRANSPARENCY           = 0,
        TRANSPARENT_BACKGROUND = bgnd,
        FONT_FACE_NAME         = "Arial",
        FONT_HEIGHT            = "15",
        HINT                   = " " .. tostring(price) .. " " .. tostring(qty)
    }
    msg.data = AddLabel(id, label)
    return msg
end

function qsfunctions.addLabel2(msg)
    local spl = split2(msg.data, "|")
    local chartTag, yValue, strDate, strTime, text, imagePath, alignment, hint, r, g, b,
          transparency, tranBackgrnd, fontName, fontHeight = unpack(spl)

    text           = (text ~= "")           and text           or nil
    imagePath      = (imagePath ~= "")      and imagePath      or nil
    alignment      = (alignment ~= "")      and alignment      or nil
    hint           = (hint ~= "")           and hint           or nil
    r              = (r ~= "-1")            and tonumber(r)    or nil
    g              = (g ~= "-1")            and tonumber(g)    or nil
    b              = (b ~= "-1")            and tonumber(b)    or nil
    transparency   = (transparency ~= "-1") and tonumber(transparency) or nil
    tranBackgrnd   = (tranBackgrnd ~= "-1") and tonumber(tranBackgrnd) or nil
    fontName       = (fontName ~= "")       and fontName       or nil
    fontHeight     = (fontHeight ~= "-1")   and fontHeight     or nil

    local labelParams = {
        YVALUE                 = yValue:gsub(",", "."),
        DATE                   = strDate,
        TIME                   = strTime,
        TEXT                   = text,
        IMAGE_PATH             = imagePath,
        ALIGNMENT              = alignment,
        HINT                   = hint,
        R                      = r or 255,
        G                      = g or 255,
        B                      = b or 255,
        TRANSPARENCY           = transparency,
        TRANSPARENT_BACKGROUND = tranBackgrnd,
        FONT_FACE_NAME         = fontName,
        FONT_HEIGHT            = fontHeight,
    }
    msg.data = AddLabel(chartTag, labelParams)
    return msg
end

function qsfunctions.setLabelParams(msg)
    local spl = split2(msg.data, "|")
    local chartTag, labelId, yValue, strDate, strTime, text, imagePath, alignment, hint, r, g, b,
          transparency, tranBackgrnd, fontName, fontHeight = unpack(spl)

    text           = (text ~= "")           and text           or nil
    imagePath      = (imagePath ~= "")      and imagePath      or nil
    alignment      = (alignment ~= "")      and alignment      or nil
    hint           = (hint ~= "")           and hint           or nil
    r              = (r ~= "-1")            and tonumber(r)    or nil
    g              = (g ~= "-1")            and tonumber(g)    or nil
    b              = (b ~= "-1")            and tonumber(b)    or nil
    transparency   = (transparency ~= "-1") and tonumber(transparency) or nil
    tranBackgrnd   = (tranBackgrnd ~= "-1") and tonumber(tranBackgrnd) or nil
    fontName       = (fontName ~= "")       and fontName       or nil
    fontHeight     = (fontHeight ~= "-1")   and fontHeight     or nil

    local labelParams = {
        YVALUE                 = yValue,
        DATE                   = strDate,
        TIME                   = strTime,
        TEXT                   = text,
        IMAGE_PATH             = imagePath,
        ALIGNMENT              = alignment,
        HINT                   = hint,
        R                      = r or 255,
        G                      = g or 255,
        B                      = b or 255,
        TRANSPARENCY           = transparency,
        TRANSPARENT_BACKGROUND = tranBackgrnd,
        FONT_FACE_NAME         = fontName,
        FONT_HEIGHT            = fontHeight,
    }
    msg.data = tostring(SetLabelParams(chartTag, tonumber(labelId), labelParams))
    return msg
end

function qsfunctions.getLabelParams(msg)
    local spl = split2(msg.data, "|")
    local chartTag, labelId = spl[1], spl[2]
    msg.data = GetLabelParams(chartTag, tonumber(labelId))
    return msg
end

function qsfunctions.delLabel(msg)
    local spl = split(msg.data, "|")
    local tag, id = spl[1], spl[2]
    DelLabel(tag, tonumber(id))
    msg.data = ""
    return msg
end

function qsfunctions.delAllLabels(msg)
    local spl = split(msg.data, "|")
    local id = spl[1]
    DelAllLabels(id)
    msg.data = ""
    return msg
end

--------------------------------------------------------------------------------
-- Классы и инструменты
--------------------------------------------------------------------------------

function qsfunctions.getClassesList(msg)
    log("Вызов " .. msg.cmd .. ", req_id=" .. tostring(msg.req_id or "?"), 0)
    local classes = getClassesList()
    if not classes or classes == "" then
        msg.data = ""
        msg.warning = "Список классов пуст или не получен"
    else
        msg.data = classes
    end
    return msg
end

function qsfunctions.getClassInfo(msg)
    msg.data = getClassInfo(msg.data.data)
    return msg
end

function qsfunctions.getClassSecurities(msg)
    msg.data = getClassSecurities(msg.data.data)
    return msg
end

function qsfunctions.getSecurityInfo(msg)
    local spl = split(msg.data.data, "|")
    msg.data = getSecurityInfo(spl[1], spl[2])
    return msg
end

function qsfunctions.getSecurityInfoBulk(msg)
    local result = {}
    for _, item in ipairs(msg.data.data) do
        local spl = split(item, "|")
        local status, sec = pcall(getSecurityInfo, spl[1], spl[2])
        table.insert(result, status and sec or json.null)
    end
    msg.data = result
    return msg
end

function qsfunctions.getSecurityClass(msg)
    local spl = split(msg.data.data, "|")
    local classes_list, sec_code = spl[1], spl[2]
    for class_code in string.gmatch(classes_list, "([^,]+)") do
        if getSecurityInfo(class_code, sec_code) then
            msg.data = class_code
            return msg
        end
    end
    msg.data = ""
    return msg
end

--------------------------------------------------------------------------------
-- Клиентские коды и торговые счета
--------------------------------------------------------------------------------


local function debug_log(...)
    if is_debug then
        log("[QS DEBUG] " .. table.concat({...}, " "),2)
    end
end

function qsfunctions.getClientCode(msg)
    local count = getNumberOf("MONEY_LIMITS") or 0
    debug_log("getClientCode: MONEY_LIMITS count =", tostring(count))

    for i = 0, count - 1 do
        local ok, item = pcall(getItem, "MONEY_LIMITS", i)

        if not ok or not item then
            debug_log("getClientCode: failed to get item at index", tostring(i))
        else
            local cc = item.client_code
            debug_log("getClientCode: index", tostring(i), "client_code =", tostring(cc))

            if cc and cc ~= "" then
                msg.data = cc
                return msg
            end
        end
    end

    debug_log("getClientCode: no client_code found")
    return msg
end

function qsfunctions.getClientCodes(msg)
    local codes = {}
    local seen = {}

    local count = getNumberOf("MONEY_LIMITS") or 0
    debug_log("getClientCodes: MONEY_LIMITS count =", tostring(count))

    for i = 0, count - 1 do
        local ok, item = pcall(getItem, "MONEY_LIMITS", i)

        if not ok or not item then
            debug_log("getClientCodes: failed at index", tostring(i))
        else
            local cc = item.client_code
            debug_log("getClientCodes: index", tostring(i), "client_code =", tostring(cc))

            if cc and cc ~= "" and not seen[cc] then
                seen[cc] = true
                table.insert(codes, cc)
            end
        end
    end

    debug_log("getClientCodes: total unique =", tostring(#codes))
    msg.data = codes
    return msg
end

function qsfunctions.getTradeAccount(msg)
    if not msg.data or msg.data == "" then
        debug_log("getTradeAccount: empty classCode")
        return msg
    end

    local count = getNumberOf("trade_accounts") or 0
    debug_log("getTradeAccount: accounts count =", tostring(count))

    for i = 0, count - 1 do
        local ok, ta = pcall(getItem, "trade_accounts", i)

        if not ok or not ta then
            debug_log("getTradeAccount: failed at index", tostring(i))
        else
            debug_log("getTradeAccount: index", tostring(i),
                "class_codes =", tostring(ta.class_codes),
                "trdaccid =", tostring(ta.trdaccid))
            if ta.class_codes and ta.trdaccid then
                if string.find(ta.class_codes, "|" .. msg.data.data .. "|", 1, true) then
                    
                    msg.data = ta.trdaccid
                    return msg
                end
            end
        end
    end

    debug_log("getTradeAccount: not found for class", tostring(msg.data))
    return msg
end

function qsfunctions.getTradeAccounts(msg)
    local accounts = {}

    local count = getNumberOf("trade_accounts") or 0
    debug_log("getTradeAccounts: accounts count =", tostring(count))

    for i = 0, count - 1 do
        local ok, ta = pcall(getItem, "trade_accounts", i)

        if not ok or not ta then
            debug_log("getTradeAccounts: failed at index", tostring(i))
        else
           
            debug_log("getTradeAccounts: index", tostring(i),
                "class_codes =", tostring(ta.class_codes))
           
            debug_log("getTradeAccounts: total valid =", tostring(#accounts))
            if ta.class_codes and ta.class_codes ~= "" then
                table.insert(accounts, ta)
            end
        end
    end

    debug_log("getTradeAccounts: total valid =", tostring(#accounts))
    msg.data = accounts
    return msg
end
--------------------------------------------------------------------------------
-- Level II котировки (стакан)
--------------------------------------------------------------------------------

function qsfunctions.Subscribe_Level_II_Quotes(msg)
    local data_str = msg.data and msg.data.data or "<nil>"

    --log("Subscribe_Level_II_Quotes called | raw data: " .. tostring(data_str), 1)

    if not msg.data or not msg.data.data or msg.data.data == "" then
        log("Subscribe_Level_II_Quotes ? пустой или некорректный msg.data.data", 3)
        msg.lua_error = "Пустой параметр для подписки"
        msg.data = false
        return msg
    end

    local spl = split(msg.data.data, "|")
    if #spl < 2 then
        log("Subscribe_Level_II_Quotes ? недостаточно параметров, получено: " .. #spl .. " частей", 3)
        log("Исходная строка: " .. msg.data.data, 3)
        msg.lua_error = "Неверный формат: ожидается class_code|sec_code"
        msg.data = false
        return msg
    end

    local class_code = spl[1]
    local sec_code   = spl[2]

    --log(string.format("Subscribe Level II: class=%s, sec=%s", class_code, sec_code), 1)

    local result = Subscribe_Level_II_Quotes(class_code, sec_code)

    -- для надёжности приводим к boolean / string
    if result == nil then
        log("Subscribe_Level_II_Quotes вернул nil", 2)
        msg.data = false
    elseif type(result) == "boolean" then
        msg.data = result
    else
        msg.data = tostring(result)
    end

    return msg
end

function qsfunctions.Unsubscribe_Level_II_Quotes(msg)
    local spl = split(msg.data.data, "|")
    msg.data = Unsubscribe_Level_II_Quotes(spl[1], spl[2])
    return msg
end

function qsfunctions.IsSubscribed_Level_II_Quotes(msg)
    local spl = split(msg.data.data, "|")
    msg.data = IsSubscribed_Level_II_Quotes(spl[1], spl[2])
    return msg
end

function qsfunctions.GetQuoteLevel2(msg)
    local spl = split(msg.data.data, "|")
    local cc, sc = spl[1], spl[2]
    local st = getInfoParam("SERVERTIME")
    local ok, data = pcall(getQuoteLevel2, cc, sc)
    if ok then
        data.class_code   = cc
        data.sec_code     = sc
        data.server_time  = st
        msg.data = data
    else
        OnError(data)
    end
    return msg
end
--------------------------------------------------------------------------------
-- Ошибки и отладка
--------------------------------------------------------------------------------

function OnError(message)
    sendError(message)
end

local function sendError(message)
    if not qsutils.is_connected() then return end

    local msg = {
        t    = timemsec(),
        cmd  = "lua_error",
        data = "Lua error: " .. tostring(message)
    }
    qsutils.sendCallback(msg)
end

--------------------------------------------------------------------------------
-- Расчёт объёма и комиссии
--------------------------------------------------------------------------------

function qsfunctions.calc_buy_sell(msg)
    local spl = split(msg.data.data, "|")
    local cc, sc, cl, acc, price, buy, market = spl[1], spl[2], spl[3], spl[4], spl[5], spl[6], spl[7]

    local is_buy    = (buy   == "True")
    local is_market = (market == "True")

    local qty, comm = CalcBuySell(cc, sc, cl, acc, tonumber(price), is_buy, is_market)
    if qty then
        msg.data = { qty = qty, commission = comm }
    else
        message("Ошибка CalcBuySell", 1)
    end
    return msg
end

--------------------------------------------------------------------------------
-- Отправка транзакций
--------------------------------------------------------------------------------

function qsfunctions.sendTransaction(msg)
    debug_log("sendTransaction вызвана")
    debug_log(" msg.data =", to_json(msg.data) )
    debug_log(" msg.data.data =", to_json(msg.data.data) )
    local res = sendTransaction(msg.data.data)
    debug_log("Result =", tostring(res))
    if res and res ~= "" then
        msg.cmd       = "lua_transaction_error"
        msg.lua_error = res
    else
        msg.data = true
    end
    return msg
end

--------------------------------------------------------------------------------
-- Параметры, депо, деньги, лимиты
--------------------------------------------------------------------------------

function qsfunctions.paramRequest(msg)
    local spl = split(msg.data.data, "|")
    local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
    msg.data = ParamRequest(class_code, sec_code, param_name)
    return msg
end

function qsfunctions.paramRequestBulk(msg)
    local result = {}
    for i = 1, #msg.data.data do
        local spl = split(msg.data.data[i], "|")
        local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
        table.insert(result, ParamRequest(class_code, sec_code, param_name))
    end
    msg.data = result
    return msg
end

function qsfunctions.cancelParamRequest(msg)
    local spl = split(msg.data.data, "|")
    local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
    msg.data = CancelParamRequest(class_code, sec_code, param_name)
    return msg
end

function qsfunctions.cancelParamRequestBulk(msg)
    local result = {}
    for i = 1, #msg.data.data do
        local spl = split(msg.data.data[i], "|")
        local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
        table.insert(result, CancelParamRequest(class_code, sec_code, param_name))
    end
    msg.data = result
    return msg
end

function qsfunctions.getParamEx(msg)
    local spl = split(msg.data.data, "|")
    local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
    msg.data = getParamEx(class_code, sec_code, param_name)
    return msg
end

function qsfunctions.getParamEx2(msg)
    local spl = split(msg.data.data, "|")
    local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
    msg.data = getParamEx2(class_code, sec_code, param_name)
    return msg
end

function qsfunctions.getParamEx2Bulk(msg)
    local result = {}
    for i = 1, #msg.data.data do
        local spl = split(msg.data.data[i], "|")
        local class_code, sec_code, param_name = spl[1], spl[2], spl[3]
        table.insert(result, getParamEx2(class_code, sec_code, param_name))
    end
    msg.data = result
    return msg
end

function qsfunctions.getDepo(msg)
    local spl = split(msg.data.data, "|")
    local clientCode, firmId, secCode, account = spl[1], spl[2], spl[3], spl[4]
    msg.data = getDepo(clientCode, firmId, secCode, account)
    return msg
end

function qsfunctions.getDepoEx(msg)
    local spl = split(msg.data.data, "|")
    local firmId, clientCode, secCode, account, limit_kind = spl[1], spl[2], spl[3], spl[4], spl[5]
    msg.data = getDepoEx(firmId, clientCode, secCode, account, tonumber(limit_kind))
    return msg
end

function qsfunctions.getMoney(msg)
    local spl = split(msg.data.data, "|")
    local client_code, firm_id, tag, curr_code = spl[1], spl[2], spl[3], spl[4]
    msg.data = getMoney(client_code, firm_id, tag, curr_code)
    return msg
end

function qsfunctions.getMoneyEx(msg)
    local spl = split(msg.data.data, "|")
    local firm_id, client_code, tag, curr_code, limit_kind = spl[1], spl[2], spl[3], spl[4], spl[5]
    msg.data = getMoneyEx(firm_id, client_code, tag, curr_code, tonumber(limit_kind))
    return msg
end

function qsfunctions.getMoneyLimits(msg)
    local limits = {}
    for i = 0, getNumberOf("money_limits") - 1 do
        table.insert(limits, getItem("money_limits", i))
    end
    msg.data = limits
    return msg
end

function qsfunctions.getFuturesLimit(msg)
    local spl = split(msg.data.data, "|")
    local firmId, accId, limitType, currCode = spl[1], spl[2], spl[3], spl[4]
    local result = getFuturesLimit(firmId, accId, limitType * 1, currCode)
    msg.data = result or nil
    return msg
end

function qsfunctions.getFuturesClientLimits(msg)
    local limits = {}
    for i = 0, getNumberOf("futures_client_limits") - 1 do
        table.insert(limits, getItem("futures_client_limits", i))
    end
    msg.data = limits
    return msg
end

function qsfunctions.getFuturesHolding(msg)
    local spl = split(msg.data.data, "|")
    local firmId, accId, secCode, posType = spl[1], spl[2], spl[3], spl[4]
    msg.data = getFuturesHolding(firmId, accId, secCode, posType * 1) or nil
    return msg
end

function qsfunctions.getFuturesClientHoldings(msg)
    local holdings = {}
    for i = 0, getNumberOf("futures_client_holding") - 1 do
        table.insert(holdings, getItem("futures_client_holding", i))
    end
    msg.data = holdings
    return msg
end

--------------------------------------------------------------------------------
-- Заявки (orders)
--------------------------------------------------------------------------------

-- function qsfunctions.get_orders(msg)
--     local class_code, sec_code = nil, nil

--     -- Разбор фильтра (class_code|sec_code)
--     if msg.data and msg.data.data and msg.data.data ~= "" then
--         local spl = split(msg.data.data, "|")
--         if #spl >= 1 then class_code = spl[1] end
--         if #spl >= 2 then sec_code = spl[2] end
--     end

--     local orders = {}
--     local count = getNumberOf("orders")

--     for i = 0, count - 1 do
--         local order = getItem("orders", i)
--         if order then
--             -- Фильтрация по инструменту (если указан)
--             if not class_code or 
--                (order.class_code == class_code and order.sec_code == sec_code) then
                
--                 -- Принудительно делаем числовой индекс (очень важно для сериализации!)
--                 table.insert(orders, order)
--             end
--         end
--     end

--     msg.data = orders

--     msg.count = #orders
--     msg.filtered = (class_code ~= nil)

--     return msg
-- end
-- Вспомогательная функция для безопасного преобразования QUIK datetime в таблицу чисел
local function quik_datetime_to_table(dt)
    if type(dt) ~= "table" then
        return nil
    end
    -- QUIK возвращает таблицу с полями: year, month, day, hour, min, sec, ms (иногда mcs)
    return {
        year      = tonumber(dt.year) or 0,
        month     = tonumber(dt.month) or 0,
        day       = tonumber(dt.day) or 0,
        hour      = tonumber(dt.hour) or 0,
        min       = tonumber(dt.min) or 0,
        sec       = tonumber(dt.sec) or 0,
        ms        = tonumber(dt.ms) or 0,
        mcs       = tonumber(dt.mcs) or 0,   -- микросекунды, если есть
        week_day  = tonumber(dt.week_day) or 0
    }
end
-- Вспомогательная функция преобразования QUIK datetime
local function quik_datetime_to_table(dt)
    if type(dt) ~= "table" then
        return nil
    end
    return {
        year     = tonumber(dt.year) or 0,
        month    = tonumber(dt.month) or 0,
        day      = tonumber(dt.day) or 0,
        hour     = tonumber(dt.hour) or 0,
        min      = tonumber(dt.min) or 0,
        sec      = tonumber(dt.sec) or 0,
        ms       = tonumber(dt.ms) or 0,
        mcs      = tonumber(dt.mcs) or 0,
        week_day = tonumber(dt.week_day) or 0
    }
end

-- Основная нормализация заявки
local function normalize_order(order)
    if not order or type(order) ~= "table" then 
        return nil 
    end

    -- === 1. Целочисленные поля (должны приходить без .0) ===
local int_fields = {
    "order_num", "ordernum", "uid", "canceled_uid", "accepted_uid", "trans_id",
    "flags", "revision_number", "trading_session", "ext_order_status", "exec_type",
    "repoterm", "linkedorder", "acnt_type", "value_entry_type", "price_entry_type",
    "qty", "qty2", "operation_type", "side_qualifier", "capacity", 
    "executing_trader_qualifier", "client_qualifier", 
    "investment_decision_maker_qualifier", "investment_decision_maker_short_code",
    "executing_trader_short_code", "client_short_code", "on_behalf_of_uid",
    "min_qty", "ext_order_flags", "passive_only_order",          -- добавлено
    "settle_date", "settle_date2", "start_date"                  -- long ? int_fields
    }
    --log("int_fields ",3)
    for _, field in ipairs(int_fields) do
        if order[field] ~= nil then
            order[field] = math.floor(tonumber(order[field]))
           -- log("field " ..  tostring(order[field]) .. " " .. tonumber(order[field]), 3)
        end
    end

    -- === 2. Дробные поля (qty, price, value и т.д.) ===
local float_fields = {
    "balance", "price", "value", "awg_price", "yield", "accruedint",
    "price2", "repo_value_balance", "repovalue", "repo2value", 
    "visible_repo_value", "visibility_factor", "start_discount",
    "visible", "external_qty",                     -- decimal в C#
    "activation_time", "expiry", "expiry_time", "repoterm",  -- decimal
    "filled_value", "value2"
    }
    --log("float_fields ",3)
    for _, field in ipairs(float_fields) do
        if order[field] ~= nil then
            order[field] = tonumber(order[field])
           -- log("field " ..  tostring(order[field]) .. " " .. tonumber(order[field]), 3)
        end
    end

    -- === 3. Строковые поля (защита от nil) ===
local string_fields = {
    "class_code", "sec_code", "account", "client_code", "brokerref", 
    "firmid", "userid", "exchange_code", "settle_currency", 
    "price_currency", "settlecode", "reject_reason", "extref", 
    "benchmark", "lseccode", "bank_acc_id", "seccode",
    "settle_date", "settlecode"   -- на всякий случай (иногда приходят как string)
}

    for _, field in ipairs(string_fields) do
        if order[field] ~= nil then
            order[field] = tostring(order[field])
        else
            order[field] = ""   -- чтобы в JSON не было null
        end
    end

    -- === 4. Даты ===
    order.datetime          = quik_datetime_to_table(order.datetime)
    order.withdraw_datetime = quik_datetime_to_table(order.withdraw_datetime)

    return order
end

-- ===================================================================
function qsfunctions.get_orders(msg)
    local class_code, sec_code = nil, nil

    if msg.data and msg.data.data and msg.data.data ~= "" then
        local spl = split(msg.data.data, "|")
        if #spl >= 1 then class_code = spl[1] end
        if #spl >= 2 then sec_code = spl[2] end
    end

    local orders = {}
    local count = getNumberOf("orders")

    for i = 0, count - 1 do
        local order = getItem("orders", i)
        if order then
            local match = true
            if class_code and sec_code then
                -- Приводим к строке на случай, если в getItem придут числа
                match = (tostring(order.class_code) == class_code and 
                         tostring(order.sec_code) == sec_code)
            end

            if match then
                local normalized = normalize_order(order)
                if normalized then
                    table.insert(orders, normalized)
                end
            end
        end
    end

    msg.data = orders
    msg.count = #orders
    msg.filtered = (class_code ~= nil)

    return msg
end

-- function qsfunctions.get_orders(msg)
--     local class_code, sec_code = nil, nil

--     debug_log("get_orders() вызвана. msg.data.data =", msg.data and msg.data.data or "nil")

--     -- Разбор фильтра (class_code|sec_code)
--     if msg.data and msg.data.data and msg.data.data ~= "" then
--         local spl = split(msg.data.data, "|")
--         if #spl >= 1 then 
--             class_code = spl[1] 
--             debug_log("  ? class_code =", class_code)
--         end
--         if #spl >= 2 then 
--             sec_code = spl[2] 
--             debug_log("  ? sec_code =", sec_code)
--         end
--     else
--         debug_log("  ? фильтр не указан (будут возвращены все заявки)")
--     end

--     local orders = {}
--     local count = getNumberOf("orders")
    
--     debug_log("  ? всего заявок в терминале:", count)

--     for i = 0, count - 1 do
--         local order = getItem("orders", i)
--         if order then
--             -- Фильтрация по инструменту (если указан)
--             local match = true
            
--             if class_code then
--                 match = (order.class_code == class_code and order.sec_code == sec_code)
                
--                 if not match then
--                     debug_log("  ? пропущена заявка #" .. i .. " (не совпадает инструмент):", 
--                               order.class_code, order.sec_code, 
--                               "? нужен:", class_code, sec_code)
--                 end
--             end

--             if match then
--                 -- Принудительно делаем числовой индекс (очень важно для сериализации!)
--                 table.insert(orders, order)
--                 debug_log("  ? добавлена заявка #" .. i .. " order_num =", order.order_num or "nil", 
--                           "status =", order.flags or "nil")
--             end
--         else
--             debug_log("  ? getItem('orders', " .. i .. ") вернул nil!")
--         end
--     end
--     debug_log("заявки ", tostring(orders))
--     msg.data = orders
--     msg.count = #orders
--     msg.filtered = (class_code ~= nil)

--     debug_log("get_orders() завершена. Возвращено заявок:", #orders, 
--               "filtered =", tostring(msg.filtered))

--     return msg
-- end

function qsfunctions.getOrder_by_ID(msg)
    local class_code, sec_code, trans_id = nil, nil, nil

    -- Разбор фильтра (class_code|sec_code|trans_id)
    if msg.data and msg.data.data and msg.data.data ~= "" then
        local spl = split(msg.data.data, "|")
        if #spl >= 1 then class_code = spl[1] end
        if #spl >= 2 then sec_code   = spl[2] end
        if #spl >= 3 then trans_id   = spl[3] end
    end

    local orders = {} 
    local count = getNumberOf("orders")

    for i = 0, count - 1 do
        local order = getItem("orders", i)
        if order then
            if order.class_code == class_code 
               and order.sec_code == sec_code 
               and order.trans_id == tonumber(trans_id) then
                
                table.insert(orders, order)
            end
        end
    end

    -- Гарантируем возврат массива (даже пустого)
    msg.data = orders
    msg.count = #orders
    msg.filtered = true

    return msg
end



function qsfunctions.get_order_by_number(msg)
    local spl = split(msg.data.data or "", "|")
    local class_code = tostring(spl[1] or "")
    local order_id   = math.tointeger(spl[2] or 0)

    if not order_id or order_id == 0 or class_code == "" then
        msg.lua_error = "Ошибка парсинга: требуется Class|OrderNum"
        msg.data = {}                   
        msg.count = 0
        return msg
    end

    local order = getOrderByNumber(class_code, order_id)

    local result = {}

    if order and order.order_num and order.order_num ~= 0 then
        local flags = math.tointeger(order.flags or 0)
        local state = 2
        if (flags & 0x1) ~= 0 then 
            state = 1 
        elseif (flags & 0x2) ~= 0 then 
            state = 3 
        end

        result = {
            order_num  = math.tointeger(order.order_num),
            sec_code   = tostring(order.sec_code),
            class_code = tostring(order.class_code),
            price      = tonumber(order.price),
            qty        = math.tointeger(order.qty),
            balance    = math.tointeger(order.balance),
            state      = state,
            datetime   = {
                year  = math.tointeger(order.datetime.year),
                month = math.tointeger(order.datetime.month),
                day   = math.tointeger(order.datetime.day),
                hour  = math.tointeger(order.datetime.hour),
                min   = math.tointeger(order.datetime.min),
                sec   = math.tointeger(order.datetime.sec)
            }
        }

        msg.count = 1
    else
        log("get_order_by_number: заявка " .. tostring(order_id) .. " не найдена", 3)
        msg.data = {}
        msg.count = 0
    end

    -- Всегда возвращаем массив (даже если одна заявка или пусто)
    msg.data = result   -- оборачиваем в массив для единообразия
    -- Если нужно возвращать просто объект при нахождении одной заявки — можно убрать фигурные скобки,
    -- но по аналогии с get_orders лучше всегда массив.

    return msg
end
--------------------------------------------------------------------------------
-- Депозитные лимиты
--------------------------------------------------------------------------------

function qsfunctions.get_depo_limits(msg)
    local filter_sec_code = tostring(msg.data.data or ""):gsub("%s+", ""):upper()

    debug_log("get_depo_limits() вызвана | фильтр:", filter_sec_code ~= "" and filter_sec_code or "<все инструменты>")

    local count = getNumberOf("depo_limits")
    debug_log("Количество записей в depo_limits:", count)

    local depo_limits = {}
    local matched = 0

    for i = 0, count - 1 do
        local d = getItem("depo_limits", i)
        if d then
            local current_sec = tostring(d.sec_code or ""):upper()

            local match = (filter_sec_code == "" or current_sec == filter_sec_code)

            if match then
                matched = matched + 1

                local item = {
                    sec_code           = tostring(d.sec_code or ""),
                    trdaccid           = tostring(d.trdaccid or ""),
                    firmid             = tostring(d.firmid or ""),
                    client_code        = tostring(d.client_code or ""),
                    openbal            = tonumber(d.openbal) or 0,
                    openlimit          = tonumber(d.openlimit) or 0,
                    currentbal         = tonumber(d.currentbal) or 0,
                    currentlimit       = tonumber(d.currentlimit) or 0,
                    locked_sell        = tonumber(d.locked_sell) or 0,
                    locked_buy         = tonumber(d.locked_buy) or 0,
                    locked_buy_value   = tonumber(d.locked_buy_value) or 0,
                    locked_sell_value  = tonumber(d.locked_sell_value) or 0,
                    awg_position_price = tonumber(d.awg_position_price) or 0,
                    limit_kind         = tonumber(d.limit_kind) or 0,
                    
                    -- Дополнительные полезные поля (часто бывают полезны при отладке)
                    balaccid           = tostring(d.balaccid or ""),
                    limit_kind_name    = (d.limit_kind == 0 and "T+2" or 
                                        d.limit_kind == 1 and "T+1" or 
                                        d.limit_kind == 2 and "T0" or tostring(d.limit_kind)),
                }

                table.insert(depo_limits, item)

                -- Детальная отладка каждой найденной позиции
                debug_log(string.format("  [%d] MATCH ? %s | currentbal: %s | currentlimit: %s | locked_sell: %s | trdaccid: %s",
                    matched,
                    item.sec_code,
                    item.currentbal,
                    item.currentlimit,
                    item.locked_sell,
                    item.trdaccid
                ))
            else
                -- Опционально: можно включить для очень подробной отладки
                -- debug_log("  SKIP ? " .. current_sec .. " (не совпадает с фильтром " .. filter_sec_code .. ")")
            end
        else
            debug_log("  WARNING: getItem('depo_limits', " .. i .. ") вернул nil!")
        end
    end

    debug_log("get_depo_limits завершена. Найдено позиций:", #depo_limits, "(matched:", matched, ")")

    -- Дополнительная проверка на пустой результат
    if #depo_limits == 0 then
        if filter_sec_code ~= "" then
            debug_log("ВНИМАНИЕ: По инструменту " .. filter_sec_code .. " депозитарных лимитов не найдено!")
        else
            debug_log("ВНИМАНИЕ: depo_limits полностью пуст!")
        end
    end

    msg.data = depo_limits
    return msg
end

-- function qsfunctions.get_depo_limits(msg)
--     local filter_sec_code = tostring(msg.data.data or ""):gsub("%s+", ""):upper()

--     local count = getNumberOf("depo_limits")
--     local depo_limits = {}

--     for i = 0, count - 1 do
--         local d = getItem("depo_limits", i)
--         if d then
--             local current_sec = tostring(d.sec_code):upper()
--             if filter_sec_code == "" or current_sec == filter_sec_code then
--                 table.insert(depo_limits, {
--                     sec_code           = tostring(d.sec_code),
--                     trdaccid           = tostring(d.trdaccid),
--                     firmid             = tostring(d.firmid),
--                     client_code        = tostring(d.client_code),
--                     openbal            = tonumber(d.openbal) or 0,
--                     openlimit          = tonumber(d.openlimit) or 0,
--                     currentbal         = tonumber(d.currentbal) or 0,
--                     currentlimit       = tonumber(d.currentlimit) or 0,
--                     locked_sell        = tonumber(d.locked_sell) or 0,
--                     locked_buy         = tonumber(d.locked_buy) or 0,
--                     locked_buy_value   = tonumber(d.locked_buy_value) or 0,
--                     locked_sell_value  = tonumber(d.locked_sell_value) or 0,
--                     awg_position_price = tonumber(d.awg_position_price) or 0,
--                     limit_kind         = tonumber(d.limit_kind) or 0
--                 })
--             end
--         end
--     end

--     msg.data = depo_limits
--     return msg
-- end

--------------------------------------------------------------------------------
-- Сделки
--------------------------------------------------------------------------------

function qsfunctions.get_trades(msg)
    local class_code, sec_code
    if msg.data.data and msg.data.data ~= "" then
        local spl = split(msg.data.data, "|")
        class_code, sec_code = spl[1], spl[2]
    end

    local trades = {}
    for i = 0, getNumberOf("trades") - 1 do
        local trade = getItem("trades", i)
        if not class_code or (trade.class_code == class_code and trade.sec_code == sec_code) then
            table.insert(trades, trade)
        end
    end
    msg.data = trades
    return msg
end

function qsfunctions.get_Trades_by_OrderNumber(msg)
    local order_num = tonumber(msg.data.data)
    local trades = {}
    for i = 0, getNumberOf("trades") - 1 do
        local trade = getItem("trades", i)
        if trade.order_num == order_num then
            table.insert(trades, trade)
        end
    end
    msg.data = trades
    return msg
end

function qsfunctions.get_all_trades(msg)
    local class_code, sec_code
    if msg.data.data and msg.data.data ~= "" then
        local spl = split(msg.data.data, "|")
        class_code, sec_code = spl[1], spl[2]
    end

    local trades = {}
    for i = 0, getNumberOf("all_trades") - 1 do
        local trade = getItem("all_trades", i)
        if not class_code or (trade.class_code == class_code and trade.sec_code == sec_code) then
            table.insert(trades, trade)
        end
    end
    msg.data = trades
    return msg
end

--------------------------------------------------------------------------------
-- Портфель
--------------------------------------------------------------------------------

function qsfunctions.getPortfolioInfo(msg)
    local spl = split(msg.data.data, "|")
    local firmId, clientCode = spl[1], spl[2]
    msg.data = getPortfolioInfo(firmId, clientCode)
    return msg
end

function qsfunctions.getPortfolioInfoEx(msg)
    local spl = split(msg.data.data, "|")
    local firmId, clientCode, limit_kind = spl[1], spl[2], spl[3]
    msg.data = getPortfolioInfoEx(firmId, clientCode, tonumber(limit_kind))
    return msg
end

--------------------------------------------------------------------------------
-- Опционы (пример реализации)
--------------------------------------------------------------------------------

function qsfunctions.getOptionBoard(msg)
    local spl = split(msg.data.data, "|")
    local classCode, secCode, series = spl[1], spl[2], spl[3]
    local result = getOptions(classCode, secCode, series)
    msg.data = result or {}
    return msg
end

function getOptions(classCode, secCode, series)
    local SecList = getClassSecurities(classCode)
    local t = {}
    local week, month, quartal, all = false, false, false, false

    for sec in string.gmatch(SecList, "([^,]+)") do
        week = false
        month = false

        local Optionbase = getParamEx(classCode, sec, "optionbase").param_image
        if string.find(secCode, Optionbase) then
            local days_to_mat = getParamEx(classCode, sec, "DAYS_TO_MAT_DATE").param_value + 0
            local len = string.len(sec)
            local last_char = string.sub(sec, len)

            if tonumber(last_char) then
                month = true
            end

            if tonumber(days_to_mat) <= 8 then
                week = true
            end

            if (tonumber(series) == 0 and week) or
               (tonumber(series) == 1 and month) or
               tonumber(series) == 4 then

                local p = {
                    code             = getParamEx(classCode, sec, "code").param_image,
                    Name             = getSecurityInfo(classCode, sec).name,
                    DAYS_TO_MAT_DATE = days_to_mat,
                    BID              = getParamEx(classCode, sec, "BID").param_value + 0,
                    OFFER            = getParamEx(classCode, sec, "OFFER").param_value + 0,
                    OPTIONBASE       = Optionbase,
                    OPTIONTYPE       = getParamEx(classCode, sec, "optiontype").param_image,
                    Longname         = getParamEx(classCode, sec, "longname").param_image,
                    shortname        = getParamEx(classCode, sec, "shortname").param_image,
                    Volatility       = getParamEx(classCode, sec, "volatility").param_value + 0,
                    Lot              = getParamEx(classCode, sec, "LOTSIZE").param_value + 0,
                    Strike           = getParamEx(classCode, sec, "strike").param_value + 0,
                    Lastprice        = getParamEx(classCode, sec, "last").param_value + 0,
                    THEORPRICE       = getParamEx(classCode, sec, "THEORPRICE").param_value + 0,
                    MAT_DATE         = getParamEx(classCode, sec, "MAT_DATE").param_image,
                    STEPPRICET       = getParamEx(classCode, sec, "STEPPRICET").param_value + 0,
                    SEC_PRICE_STEP   = getParamEx(classCode, sec, "SEC_PRICE_STEP").param_value + 0
                }
                table.insert(t, p)
            end
        end
    end
    return t
end

--------------------------------------------------------------------------------
-- Стоп-заявки
--------------------------------------------------------------------------------

function qsfunctions.get_stop_orders(msg)
    local class_code, sec_code
    if msg.data.data and msg.data.data ~= "" then
        local spl = split(msg.data.data, "|")
        class_code, sec_code = spl[1], spl[2]
    end

    local count = getNumberOf("stop_orders")
    local stop_orders = {}
    for i = 0, count - 1 do
        local so = getItem("stop_orders", i)
        if not class_code or (so.class_code == class_code and so.sec_code == sec_code) then
            table.insert(stop_orders, so)
        end
    end
    msg.data = stop_orders
    return msg
end

--------------------------------------------------------------------------------
-- Свечи
--------------------------------------------------------------------------------

function qsfunctions.get_num_candles(msg)
    local spl = split(msg.data.data, "|")
    local tag = spl[1]
    msg.data = getNumCandles(tag) * 1
    return msg
end

function qsfunctions.get_candles(msg)
    local spl = split(msg.data.data, "|")
    local tag          = spl[1]
    local line         = tonumber(spl[2]) or 0
    local first_candle = tonumber(spl[3]) or 0
    local count        = tonumber(spl[4]) or 0

    if count == 0 then
        count = getNumCandles(tag) * 1
    end

    local t, n = getCandlesByIndex(tag, line, first_candle, count)

    local candles = {}
    if t and n > 0 then
        for i = 1, n do
            local c = t[i-1]
            if c then
                table.insert(candles, {
                    open   = tonumber(c.open),
                    close  = tonumber(c.close),
                    high   = tonumber(c.high),
                    low    = tonumber(c.low),
                    volume = tonumber(c.volume),
                    datetime = {
                        year  = tonumber(c.datetime.year),
                        month = tonumber(c.datetime.month),
                        day   = tonumber(c.datetime.day),
                        hour  = tonumber(c.datetime.hour),
                        min   = tonumber(c.datetime.min),
                        sec   = tonumber(c.datetime.sec)
                    }
                })
            end
        end
    end

    msg.data = candles
    return msg
end

--------------------------------------------------------------------------------
-- DataSource (подписка на свечи)
--------------------------------------------------------------------------------

data_sources = data_sources or {}
last_indexes = last_indexes or {}

local function get_key(class, sec, interval)
    return class .. "|" .. sec .. "|" .. tostring(interval)
end

local function fetch_candle(ds, index)
    return {
        open   = ds:O(index),
        high   = ds:H(index),
        low    = ds:L(index),
        close  = ds:C(index),
        volume = ds:V(index),
        datetime = ds:T(index)
    }
end

local function data_source_callback(index, class, sec, interval)
    local key = get_key(class, sec, interval)
    if data_sources[key] and index ~= last_indexes[key] then
        last_indexes[key] = index
        local candle = fetch_candle(data_sources[key], index - 1)
        if candle then
            local clean_candle = {
                open     = tonumber(candle.open),
                close    = tonumber(candle.close),
                high     = tonumber(candle.high),
                low      = tonumber(candle.low),
                volume   = tonumber(candle.volume),
                sec      = tostring(sec),
                class    = tostring(class),
                interval = tonumber(interval),
                datetime = {
                    year  = tonumber(candle.datetime.year),
                    month = tonumber(candle.datetime.month),
                    day   = tonumber(candle.datetime.day),
                    hour  = tonumber(candle.datetime.hour),
                    min   = tonumber(candle.datetime.min),
                    sec   = tonumber(candle.datetime.sec)
                }
            }

            local msg = {
                t    = timemsec(),
                cmd  = "NewCandle",
                data = clean_candle
            }
            sendCallback(msg)
        end
    end
end

function qsfunctions.subscribe_to_candles(msg)
    local spl = split(msg.data.data, "|")
    local class, sec, interval = spl[1], spl[2], tonumber(spl[3])

    local ds, err = CreateDataSource(class, sec, interval)
    if err then
        msg.cmd = "lua_create_data_source_error"
        msg.lua_error = err
        return msg
    end

    local attempts = 0
    repeat
        sleep(100)
        attempts = attempts + 1
    until ds:Size() > 0 or attempts >= 30

    if ds:Size() == 0 then
        ds:Close()
        msg.data = "error"
        msg.lua_error = "DataSource пустой после ожидания"
        return msg
    end

    local key = get_key(class, sec, interval)
    data_sources[key] = ds
    last_indexes[key] = ds:Size()

    ds:SetUpdateCallback(function(idx)
        data_source_callback(idx, class, sec, interval)
    end)

    msg.data = "success"
    return msg
end

function qsfunctions.unsubscribe_from_candles(msg)
    local spl = split(msg.data.data, "|")
    local class, sec, interval = spl[1], spl[2], tonumber(spl[3])
    local key = get_key(class, sec, interval)

    if data_sources[key] then
        data_sources[key]:Close()
        data_sources[key]  = nil
        last_indexes[key]  = nil
        msg.data = "success"
    else
        msg.data = "not subscribed"
    end
    return msg
end

function qsfunctions.is_subscribed(msg)
    local spl = split(msg.data.data, "|")
    local class, sec, interval = spl[1], spl[2], tonumber(spl[3])
    local key = get_key(class, sec, interval)
    msg.data = data_sources[key] ~= nil
    return msg
end

--------------------------------------------------------------------------------

return qsfunctions

-- vim: ts=4 sts=4 sw=4 et