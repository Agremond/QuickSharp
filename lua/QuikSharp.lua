-- QuikSharp.lua
-- Главный скрипт QUIK# с использованием shared memory (ipc.shm + ipc.sem)
-- Адаптировано под qsutils.lua (версия с connect / receiveRequest / sendResponse)

local util = require "qsutils"
local json = require "dkjson"           -- если нужно вручную

-- Подключаем колбэки и функции QUIK (если файлы существуют)
local qf = require "qsfunctions"        -- обработка команд
local callbacks = require "qscallbacks" -- обработка событий

-- Определяем, запущены ли мы в QUIK
function is_quik()
    return getScriptPath ~= nil
end

quikVersion = nil
script_path = "."

if is_quik() then
    script_path = getScriptPath()
    
    quikVersion = getInfoParam("VERSION")
    if quikVersion then
        local t = {}
        for str in string.gmatch(quikVersion, "([^%.]+)") do
            table.insert(t, str)
        end
        quikVersion = tonumber(t[1]) * 100 + tonumber(t[2])
    end
    
    if quikVersion == nil then
        message("QUIK# cannot detect QUIK version", 3)
        return
    end
    
    local linkage = "MD"
    local libPath
    
    if quikVersion >= 805 then
        libPath = "\\clibs64\\53_" .. linkage .. "\\"
    elseif quikVersion >= 800 then
        libPath = "\\clibs64\\5.1_" .. linkage .. "\\"
    else
        libPath = "\\clibs\\5.1_" .. linkage .. "\\"
    end
    
    package.path  = package.path  .. ";" .. script_path .. "\\?.lua;" .. script_path .. "\\?.luac"
    package.cpath = package.cpath .. ";" .. script_path .. libPath .. "?.dll;" .. "." .. libPath .. "?.dll"
end

log("Detected Quik version: " .. (quikVersion or "unknown") .. ", script path: " .. script_path, 0)

-- Глобальный флаг работы скрипта
function IsScriptRunning()
    return getScriptPath() ~= nil
end

--- Главная функция (QUIK вызывает автоматически)
function main()
    message("QuikSharp: запуск...", 1)

    local connected = util.connect()
    if not connected then
        message("QuikSharp: не удалось инициализировать shared memory", 3)
        return
    end

    message("QuikSharp: IPC (shared memory) успешно инициализирован", 1)

    while IsScriptRunning() do
        local cmd, req_id, err = util.receiveRequest(0.050)   -- 50 мс — комфортный баланс

        if cmd then

            -- --------------------------------
            -- Нормальный запрос с телом
            -- --------------------------------
            log("Запрос от C# (req_id="..tostring(req_id).."): " .. to_json(cmd), 0)
		
            local result = qf.dispatch_and_process(cmd)
		if cmd.nonce then
		    result.nonce = cmd.nonce   -- копируем обратно в ответ
		end
		result.req_id = req_id
log("После dispatch: cmd=" .. cmd.cmd .. ", data тип=" .. type(result.data) .. ", data=" .. to_json(result.data or {}), 1)
            local ok, send_err = util.sendResponse(result)
            if not ok then
                log("Ошибка отправки ответа: " .. tostring(send_err), 2)
            end

        elseif err == "timeout" then
            -- Обычное дело — просто ждём дальше
            sleep(5)

        elseif err == "empty body" then
            -- --------------------------------
            -- Пустое сообщение — можно ответить pong / heartbeat
            -- --------------------------------
            local response = {
                cmd     = "heartbeat",
                req_id  = req_id,
                t       = timemsec(),
                success = true
            }
            local ok, send_err = util.sendResponse(response)
            if not ok then
                log("Ошибка отправки heartbeat: " .. tostring(send_err), 2)
            end

        else
            -- --------------------------------
            -- Ошибка чтения / парсинга / магии
            -- --------------------------------
            if err then
                log("Ошибка приёма запроса: " .. tostring(err), 2)
            end
            sleep(10)   -- небольшая пауза перед следующей попыткой
        end
    end

    util.Close()
    message("QuikSharp: скрипт остановлен", 1)
end

-- Стандартные QUIK-колбэки
function OnInit()
    -- можно добавить дополнительную инициализацию, если нужно
end

function OnStop()
    util.Close()
    message("QuikSharp: OnStop > IPC закрыт", 1)
end

-- Примеры колбэков QUIK > C#
function OnOrder(order)
    if callbacks and callbacks.OnOrder then
        local data = callbacks.OnOrder(order)
        util.sendCallback(data)
    end
end

function OnTrade(trade)
    if callbacks and callbacks.OnTrade then
        local data = callbacks.OnTrade(trade)
        util.sendCallback(data)
    end
end

-- Добавь другие события по необходимости:
-- OnParam, OnStopOrder, OnMoneyLimits, OnDepoLimits, OnFuturesClientHolding и т.д.

message("QuikSharp загружен и готов к работе (SHM-версия)", 1)