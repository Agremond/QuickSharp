-- test_orders_dump.lua
-- Диагностика: почему GetOrderByTransIdAsync (Lua-обработчик getOrder_by_ID в qsfunctions.lua)
-- не находит реально существующую, видимую в самом QUIK заявку, хотя таблица "Заявки" точно
-- открыта. getOrder_by_ID ищет по трём полям одновременно: class_code, sec_code, trans_id
-- (см. qsfunctions.lua:1051-1081) — этот скрипт выгружает ВСЕ поля ВСЕХ заявок из той же самой
-- таблицы QUIK (getNumberOf("orders")/getItem("orders", i)), которую использует тот же
-- getOrder_by_ID, чтобы увидеть, что там РЕАЛЬНО лежит, и сравнить с тем, что ожидает C#-сторона
-- (PendingLegOrder.TransId, PendingLegOrder.ClassCode = "SPBOPT"/"SPBFUT", ActiveLeg.InstrumentCode).
--
-- Ноль зависимостей от QuikSharp — тот же принцип, что и test_transreply.lua.
--
-- КАК ИСПОЛЬЗОВАТЬ:
-- 1. Файл лежит в D:\QUIK_VTB\lua\test_orders_dump.lua — рядом с рабочим QuikSharp-скриптом.
-- 2. В QUIK: Сервис -> Настройки -> Lua-скрипты -> добавить этот файл ДОПОЛНИТЕЛЬНЫМ скриптом
--    (можно держать запущенным одновременно с основным QuikSharp — независимые окружения).
-- 3. Запустить скрипт.
-- 4. Через приложение (в Prod-режиме) отправить любую заявку, которая должна повиснуть в
--    Filling — то же самое, на чём воспроизводится баг ("не появилась в реестре QUIK за 30с").
-- 5. Пока заявка висит, смотреть D:\QUIK_VTB\lua\test_orders_dump.log (или журнал сообщений
--    QUIK) — каждые 5 секунд скрипт печатает ПОЛНЫЙ дамп всех полей ВСЕХ текущих заявок из
--    таблицы, плюс отдельно — мгновенный дамп по каждому событию OnOrder.
-- 6. Сравнить: (а) есть ли в дампе периодического опроса заявка с нужным trans_id вообще —
--    если ДАЖЕ ЕЁ TАМ НЕТ, значит getNumberOf("orders")/getItem("orders", i) в принципе не
--    видят эту заявку (не баг сравнения полей, а более глубокая проблема — например, заявка
--    относится к другому счёту/фирме, не входящему в выборку этой таблицы для этого скрипта);
--    (b) если заявка ЕСТЬ в дампе — сравнить ТОЧНЫЕ значения class_code/sec_code/trans_id
--    построчно с тем, что ожидает наш код (SPBOPT/SPBFUT, код инструмента, TransId из лога
--    приложения "Заявка по ноге ... (TransId #NNNNNNN)") — расхождение сразу будет видно
--    (лишние пробелы, другой регистр, другой формат кода класса и т.д.).

local log_path = (getScriptPath and getScriptPath() or ".") .. "\\test_orders_dump.log"

local function log(msg)
    local line = os.date("%Y-%m-%d %H:%M:%S") .. "  " .. msg

    local f = io.open(log_path, "a")
    if f then
        f:write(line .. "\n")
        f:close()
    end

    if message then
        pcall(message, "[test_orders_dump] " .. msg, 1)
    end
end

--- Печатает ВСЕ поля произвольной QUIK-таблицы (order/trade/...) одной строкой,
--- без предположений о конкретном наборе полей — если C#-сторона ждёт поле, которого тут
--- нет, или оно называется иначе, это сразу будет видно.
local function dump_fields(prefix, tbl)
    local parts = {}
    for k, v in pairs(tbl) do
        table.insert(parts, tostring(k) .. "=" .. tostring(v))
    end
    table.sort(parts) -- стабильный порядок между вызовами, легче сравнивать глазами
    log(prefix .. "{ " .. table.concat(parts, ", ") .. " }")
end

local function dump_all_orders()
    local ok, count = pcall(getNumberOf, "orders")
    if not ok then
        log("getNumberOf('orders') FAILED: " .. tostring(count))
        return
    end

    log(string.format("--- Периодический опрос: getNumberOf('orders') = %s ---", tostring(count)))

    for i = 0, count - 1 do
        local ok2, order = pcall(getItem, "orders", i)
        if ok2 and order then
            dump_fields(string.format("  orders[%d]: ", i), order)
        else
            log(string.format("  orders[%d]: getItem FAILED: %s", i, tostring(order)))
        end
    end
end

function main()
    local ver = getInfoParam and getInfoParam("VERSION") or "?"
    log("=== main() started === QUIK version=" .. tostring(ver))

    while true do
        dump_all_orders()
        sleep(5000)
    end
end

function OnInit(script_path)
    log("OnInit called, path=" .. tostring(script_path))
end

function OnStop(signal)
    log("OnStop called, signal=" .. tostring(signal))
    return 1000
end

function OnConnected()
    log("OnConnected")
end

function OnDisconnected()
    log("OnDisconnected")
end

-- Мгновенный дамп в момент самого события — не ждать следующего 5-секундного опроса.
function OnOrder(order)
    dump_fields("OnOrder FIRED: ", order)
end

log("--- Script file loaded (top-level code executed) ---")
