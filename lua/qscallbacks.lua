-- qscallbacks.lua
-- Обработчики событий (callbacks) QUIK ? внешний процесс (C#/Python/...)
-- Все функции, начинающиеся с On..., автоматически вызываются QUIK-ом

package.path = package.path .. ";.\\?.lua;.\\?.luac"

local qsutils = require "qsutils"

local qscallbacks = {}

--------------------------------------------------------------------------------
-- Вспомогательные функции
--------------------------------------------------------------------------------

local function sendError(message)
    if not qsutils.is_connected() then return end

    local msg = {
        t    = timemsec(),
        cmd  = "lua_error",
        data = "Lua error: " .. tostring(message)
    }
    qsutils.sendCallback(msg)
end

local function sendEvent(cmd, data)
    if not qsutils.is_connected() then return end

    local msg = {
        t   = timemsec(),
        cmd = cmd,
        data = data or ""
    }
    qsutils.sendCallback(msg)
end

local function CleanUp()
    -- Закрытие лога и других ресурсов при завершении
    if closeLog then
        pcall(closeLog)
    end
end

--------------------------------------------------------------------------------
-- Основные события подключения / отключения
--------------------------------------------------------------------------------

function OnConnected()
    sendEvent("OnConnected")
end

function OnDisconnected()
    sendEvent("OnDisconnected")
end

function OnQuikSharpDisconnected()
    -- Здесь можно добавить логику восстановления / управления рисками
    -- (например, закрыть позиции, отменить заявки и т.д.)
    log("QuikSharp IPC disconnected", 2)
end

--------------------------------------------------------------------------------
-- Ошибки и отладка
--------------------------------------------------------------------------------

function OnError(message)
    sendError(message)
end

--------------------------------------------------------------------------------
-- Инициализация и завершение скрипта
--------------------------------------------------------------------------------

function OnInit(script_path)
    sendEvent("OnInit", script_path)
    log("QUIK# initialized from: " .. tostring(script_path), 1)
end

function OnClose()
    sendEvent("OnClose")
    CleanUp()
end

function OnStop(s)
    is_started = false

    sendEvent("OnStop", s)

    log("QUIK# stopped. Script will auto-start on next QUIK launch if left running.", 1)
    CleanUp()

    -- Рекомендуемое QUIK-ом значение задержки перед полным выходом
    return 1000
end

--------------------------------------------------------------------------------
-- Сделки и заявки
--------------------------------------------------------------------------------

function OnAllTrade(alltrade)
    sendEvent("OnAllTrade", alltrade)
end

function OnTrade(trade)
    sendEvent("OnTrade", trade)
end

function OnOrder(order)
    sendEvent("OnOrder", order)
end

function OnStopOrder(stop_order)
    sendEvent("OnStopOrder", stop_order)
end

function OnTransReply(trans_reply)
    sendEvent("OnTransReply", trans_reply)
end

--------------------------------------------------------------------------------
-- Стакан (Level II), параметры инструмента
--------------------------------------------------------------------------------

function OnQuote(class_code, sec_code)
    if not qsutils.is_connected() then return end

    local server_time = getInfoParam("SERVERTIME") or ""

    local status, ql2 = pcall(getQuoteLevel2, class_code, sec_code)

    if status then
        ql2.class_code   = class_code
        ql2.sec_code     = sec_code
        ql2.server_time  = server_time

        sendEvent("OnQuote", ql2)
    else
        sendError(ql2 or "getQuoteLevel2 failed")
    end
end

function OnParam(class_code, sec_code)
    sendEvent("OnParam", {
        class_code = class_code,
        sec_code   = sec_code
    })
end

--------------------------------------------------------------------------------
-- Лимиты, позиции, счета
--------------------------------------------------------------------------------

function OnAccountBalance(acc_bal)
    sendEvent("OnAccountBalance", acc_bal)
end

function OnAccountPosition(acc_pos)
    sendEvent("OnAccountPosition", acc_pos)
end

function OnDepoLimit(dlimit)
    sendEvent("OnDepoLimit", dlimit)
end

function OnDepoLimitDelete(dlimit_del)
    sendEvent("OnDepoLimitDelete", dlimit_del)
end

function OnMoneyLimit(mlimit)
    sendEvent("OnMoneyLimit", mlimit)
end

function OnMoneyLimitDelete(mlimit_del)
    sendEvent("OnMoneyLimitDelete", mlimit_del)
end

function OnFuturesLimitChange(fut_limit)
    sendEvent("OnFuturesLimitChange", fut_limit)
end

function OnFuturesLimitDelete(lim_del)
    sendEvent("OnFuturesLimitDelete", lim_del)
end

function OnFuturesClientHolding(fut_pos)
    sendEvent("OnFuturesClientHolding", fut_pos)
end

--------------------------------------------------------------------------------
-- Прочие события (фирмы, редко используемые)
--------------------------------------------------------------------------------

function OnFirm(firm)
    sendEvent("OnFirm", firm)
end

--------------------------------------------------------------------------------

return qscallbacks

-- vim: ts=4 sts=4 sw=4 et