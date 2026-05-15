-- QuikSharp.lua
-- Главный скрипт QUIK# с использованием shared memory (ipc.shm + ipc.sem)
-- Использует qsutils.lua (работа с connect / receiveRequest / sendResponse)

local util = require "qsutils"
local json = require "dkjson"           -- Если нужно парсить

-- Регистрация функций в системе QUIK (если нужно разделение)
local qf = require "qsfunctions"        -- Обработка команд
local callbacks = require "qscallbacks" -- Обработка событий

-- Проверка, запущен ли скрипт в QUIK
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

-- Проверка статуса выполнения скрипта
function IsScriptRunning()
    return getScriptPath() ~= nil
end

--- Главная функция (QUIK вызывает автоматически)
function main()
    message("QuikSharp: Запуск...", 1)

    local connected = util.connect()
    if not connected then
        message("QuikSharp: Не удалось инициализировать shared memory", 3)
        return
    end

    message("QuikSharp: IPC (shared memory) успешно инициализирован", 1)

    while IsScriptRunning() do
        local cmd, req_id, err = util.receiveRequest(0.150)   -- 50 мс в ожидании данных

        if cmd then
		
            -- --------------------------------
            -- Обработка команд от C#
            -- --------------------------------
            --log("Запрос от C# 1 (req_id=" .. tostring(req_id).."): " .. to_json(cmd), 0)
	    if cmd.cmd == "ping" then
            -- Специальная обработка ping с ответом как-нибудь
            response = {
                cmd    = "ping",
                req_id = req_id,          -- Обязательно возвращаем тот же id
                data   = "Pong",          -- Или "Ping", "OK", "" в зависимости
                t      = timemsec(),
                success = true
            }
        else	
	        local result = qf.dispatch_and_process(cmd)
		if cmd.nonce then
		    result.nonce = cmd.nonce   -- Добавляем обратно в ответ
        end
		result.req_id = req_id
		--log("После dispatch: cmd=" .. tostring(result.cmd) .. ", data тип=" .. type(result.data), 1)
	        local ok, send_err = util.sendResponse(result)
	        if not ok then
	            log("Ошибка отправки ответа: " .. tostring(send_err), 2)
	        end
	   end

        elseif err == "timeout" then
            -- Тишина, идем в начало или просто ждем
            sleep(5)

        elseif err == "empty body" then
            -- --------------------------------
            -- Пример структуры в стиле заголовок pong / heartbeat
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
            -- Прочие ошибки / события / вылет
            -- --------------------------------
            if err then
                log("Ошибка цикла событий: " .. tostring(err), 2)
            end
            sleep(10)   -- Небольшая пауза перед повторной попыткой
        end
    end

    util.Close()
    message("QuikSharp: Работа завершена", 1)
end

-- Обязательные QUIK-функции
function OnInit()
    -- Здесь возможна дополнительная инициализация, если нужно
end

function OnStop()
    util.Close()
    message("QuikSharp: OnStop > IPC закрыт", 1)
end

-- Функции обратного вызова QUIK > C#
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

-- Другие нужные события по аналогии:
-- OnParam, OnStopOrder, OnMoneyLimits, OnDepoLimits, OnFuturesClientHolding и т.д.

message("QuikSharp готов к связи в памяти (SHM-режим)", 1)