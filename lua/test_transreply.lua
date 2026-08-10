-- test_transreply.lua
-- Минимальный диагностический скрипт: проверяет, вызывает ли QUIK колбэк OnTransReply
-- НАТИВНО, в обход всей цепочки QuikSharp (require qsutils/dkjson/ipc.shm и т.д.) — ноль
-- внешних зависимостей, только сам Lua и функции самого QUIK (message, sleep, getScriptPath).
--
-- ЗАЧЕМ: OnOrder уже подтверждён рабочим (после фикса QuikSharp.lua), а OnTransReply —
-- по-прежнему нет, хотя объявлен структурно идентично. Этот скрипт проверяет ту же гипотезу
-- в чистом виде, без QuikSharp вообще, чтобы окончательно исключить (или подтвердить) вопрос
-- "дело в нашем коде" vs "дело в самом QUIK/настройках/версии".
--
-- КАК ИСПОЛЬЗОВАТЬ:
-- 1. Файл уже лежит в D:\QUIK_VTB\lua\test_transreply.lua — рядом с рабочим QuikSharp-скриптом.
-- 2. В QUIK: Сервис -> Настройки -> вкладка со списком Lua-скриптов (там же, где подключён
--    основной QuikSharp/autostart-скрипт) -> добавить этот файл как ОТДЕЛЬНЫЙ, ДОПОЛНИТЕЛЬНЫЙ
--    скрипт (галка "Автозапуск" не обязательна — можно запустить вручную кнопкой "Запустить").
--    Каждый Lua-скрипт в QUIK выполняется в своём изолированном окружении, так что этот скрипт
--    никак не помешает основному QuikSharp — можно держать оба запущенными одновременно.
-- 3. Запустить скрипт (если не автозапуск — кнопкой в том же диалоге).
-- 4. Отправить любую транзакцию — вариант А: прямо в самом QUIK вручную выставить и тут же
--    снять заявку (стакан -> купить/продать -> отменить); вариант Б: через наше C#-приложение
--    (можно параллельно, без остановки других скриптов).
-- 5. Смотреть D:\QUIK_VTB\lua\test_transreply.log (создастся автоматически рядом со скриптом)
--    и/или журнал сообщений QUIK (там же, где обычно видны сообщения от скриптов) — туда тоже
--    дублируется каждая строка.
--
-- КАК ЧИТАТЬ РЕЗУЛЬТАТ:
-- - Если после отправки транзакции в логе появилась строка "OnTransReply FIRED" —
--   значит QUIK нативно вызывает колбэк, и проблема была именно в нашем коде/цепочке require
--   (в самом QuikSharp-скрипте, а не тут) — надо копать дальше именно там.
-- - Если "OnOrder FIRED" есть, а "OnTransReply FIRED" — нет, ни разу, ни для одной
--   транзакции — значит дело не в коде вообще, а в самом QUIK: терминал этой версии/сборки,
--   или конкретная настройка (права скрипта, тип подключения, версия qlua.dll и т.п.) не
--   отдаёт этому терминалу/скрипту OnTransReply в принципе. Тогда это вопрос к документации
--   QUIK под вашу версию (лог покажет её — см. строку "main() started") или к брокеру/техподдержке.

local log_path = (getScriptPath and getScriptPath() or ".") .. "\\test_transreply.log"

local function log(msg)
    local line = os.date("%Y-%m-%d %H:%M:%S") .. "  " .. msg

    local f = io.open(log_path, "a")
    if f then
        f:write(line .. "\n")
        f:close()
    end

    -- Дублируем в журнал сообщений самого QUIK, чтобы видеть сразу, не открывая файл
    if message then
        pcall(message, "[test_transreply] " .. msg, 1)
    end
end

function main()
    local ver = getInfoParam and getInfoParam("VERSION") or "?"
    log("=== main() started === QUIK version=" .. tostring(ver) .. ", script_path=" .. tostring(getScriptPath and getScriptPath() or "?"))

    while true do
        sleep(1000)
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
    log("OnConnected (соединение терминала с сервером QUIK установлено)")
end

function OnDisconnected()
    log("OnDisconnected")
end

-- Контрольная проверка — этот колбэк УЖЕ подтверждён рабочим в основном QuikSharp-скрипте,
-- здесь просто для сравнения: если он не сработает даже тут — подозрение падёт уже не на
-- конкретно OnTransReply, а вообще на то, что этот скрипт неправильно подключён в QUIK.
function OnOrder(order)
    log(string.format(
        "OnOrder FIRED: order_num=%s class_code=%s sec_code=%s price=%s qty=%s flags=%s",
        tostring(order.order_num), tostring(order.class_code), tostring(order.sec_code),
        tostring(order.price), tostring(order.qty), tostring(order.flags)))
end

function OnTrade(trade)
    log(string.format(
        "OnTrade FIRED: order_num=%s price=%s qty=%s",
        tostring(trade.order_num), tostring(trade.price), tostring(trade.qty)))
end

-- ГЛАВНАЯ ПРОВЕРКА
function OnTransReply(trans_reply)
    log(string.format(
        "OnTransReply FIRED: trans_id=%s status=%s result_msg=%s order_num=%s error_code=%s error_source=%s",
        tostring(trans_reply.trans_id), tostring(trans_reply.status), tostring(trans_reply.result_msg),
        tostring(trans_reply.order_num), tostring(trans_reply.error_code), tostring(trans_reply.error_source)))
end

log("--- Script file loaded (top-level code executed) ---")
