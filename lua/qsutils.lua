-- qsutils.lua
local json = require "dkjson"
local ipcshm = require "ipc.shm"  -- ipc.shm для shared memory
local ipcsem = require "ipc.sem"  -- ipc.sem для семафоров

local qsutils = {}

--- Sleep that always works
function delay(msec)
    if sleep then
        pcall(sleep, msec)
    else
        -- pcall(socket.select, nil, nil, msec / 1000)
    end
end

script_path = getScriptPath()
local time_offset = 0
local last_os_time = os.time()

function timemsec()
    local now = os.time()
    if now > last_os_time then
        time_offset = 0
        last_os_time = now
    end
    time_offset = time_offset + 50 -- грубая эмуляция
    return (now * 1000) + time_offset % 1000
end

-- Returns the name of the file that calls this function (without extension)
function scriptFilename()
    -- Check that Lua runtime was built with debug information enabled
    if not debug or not debug.getinfo then
        return nil
    end
    local full_path = debug.getinfo(2, "S").source:sub(2)
    return string.gsub(full_path, "^.*\\(.*)[.]lua[c]?$", "%1")
end

is_debug = false

-- log files
function openLog()
    os.execute("mkdir \""..script_path.."\\logs\"")
    local lf = io.open (script_path.. "\\logs\\QUIK#_"..os.date("%Y%m%d")..".log", "a")
    if not lf then
        lf = io.open (script_path.. "\\QUIK#_"..os.date("%Y%m%d")..".log", "a")
    end
    return lf
end

-- Returns contents of config.json file or nil if no such file exists
function readConfigAsJson()
    local conf = io.open (script_path.. "\\config.json", "r")
    if not conf then
        return nil
    end
    local content = conf:read "*a"
    conf:close()
    return from_json(content)
end

function paramsFromConfig(scriptName)
    local params = {}
    -- just default values
    table.insert(params, "127.0.0.1") -- responseHostname
    table.insert(params, 34130) -- responsePort
    table.insert(params, "127.0.0.1") -- callbackHostname
    table.insert(params, 34131) -- callbackPort
    local config = readConfigAsJson()
    if not config or not config.servers then
        return nil
    end
    local found = false
    for i=1,#config.servers do
        local server = config.servers[i]
        if server.scriptName == scriptName then
            found = true
            if server.responseHostname then
                params[1] = server.responseHostname
            end
            if server.responsePort then
                params[2] = server.responsePort
            end
            if server.callbackHostname then
                params[3] = server.callbackHostname
            end
            if server.callbackPort then
                params[4] = server.callbackPort
            end
        end
    end
    if found then
        return params
    else
        return nil
    end
end

--- Write to log file and to Quik messages
function log(msg, level)
    if not msg then msg = "" end
    if level == 1 or level == 2 or level == 3 or is_debug then
        -- only warnings and recoverable errors to Quik
        if message then
            pcall(message, msg, level)
        end
    end
    if not level then level = 0 end
    local logLine = "LOG "..level..": "..msg
    print(logLine)
    local msecs = math.floor(math.fmod(timemsec(), 1000));
    if logfile then
        pcall(logfile.write, logfile, os.date("%Y-%m-%d %H:%M:%S").."."..msecs.." "..logLine.."\n")
        pcall(logfile.flush, logfile)
    end
end

-- Doesn't work if string contains empty values, eg. 'foo,,bar'. You get {'foo','bar'} instead of {'foo', '', 'bar'}
function split(inputstr, sep)
    if sep == nil then
        sep = "%s"
    end
    local t={}
    local i=1
    for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
        t[i] = str
        i = i + 1
    end
    return t
end

-- https://stackoverflow.com/questions/1426954/split-string-in-lua#comment73602874_7615129
function split2(inputstr, sep)
    sep = sep or '%s'
    local t = {}
    for field, s in string.gmatch(inputstr, "([^"..sep.."]*)("..sep.."?)") do
        table.insert(t, field)
        if s == "" then
            return t
        end
    end
end

function from_json(str)
    local status, msg= pcall(json.decode, str, 1, json.null) -- dkjson
    if status then
        return msg
    else
        return nil, msg
    end
end

function to_json(msg)
    local status, str= pcall(json.encode, msg, { indent = false }) -- dkjson
    if status then
        return str
    else
        error(str)
    end
end

-- =============================================================================
-- ТРАНСПОРТ: shared memory + семафоры + mutex (Вариант 2: отдельные буферы)
-- =============================================================================

-- Имена объектов (глобальные в системе)
local REQ_SHM_NAME = "QuikSharp_Request_Shmem"  -- Для запросов C# -> Lua
local RESP_SHM_NAME = "QuikSharp_Response_Shmem"  -- Для синхронных ответов Lua -> C#
local CB_SHM_NAME = "QuikSharp_Callback_Shmem"  -- Для асинхронных callbacks Lua -> C#

local REQ_SEM_NAME = "QuikSharp_Request_Sem"  -- Сигнал о новом запросе (C# post)
local RESP_SEM_NAME = "QuikSharp_Response_Sem"  -- Сигнал о новом ответе (Lua post)
local CB_SEM_NAME = "QuikSharp_Callback_Sem"  -- Сигнал о новом callback (Lua post)

local REQ_MTX_NAME = "QuikSharp_Request_MutexSem"  -- Защита Request shmem
local RESP_MTX_NAME = "QuikSharp_Response_MutexSem"  -- Защита Response shmem
local CB_MTX_NAME = "QuikSharp_Callback_MutexSem"  -- Защита Callback shmem

-- Размеры буферов (4MB общий в оригинале — разделим: 1MB request, 1MB response, 2MB callback для объёмных данных)
local SHM_SIZE_REQ = 1024 * 1024  -- 1MB
local SHM_SIZE_RESP = 1024 * 1024  -- 1MB
local SHM_SIZE_CB = 2 * 1024 * 1024  -- 2MB

local HEADER_SIZE = 24  -- Как в оригинале: 6x uint32 (magic, ver, req_id, msg_type, body_len, reserved)
local MAGIC = 0x5155494B  -- "QUIK"
local VERSION = 2

-- Хэндлы ресурсов
local req_shm, resp_shm, cb_shm
local req_sem, resp_sem, cb_sem
local req_mtx, resp_mtx, cb_mtx

local is_connected = false

local function init_shm()
    if req_shm then return true end  -- Уже инициализировано

    -- Создаём/открываем shared memory (create — если не существует, создаст; иначе откроет)
    local ok, err = ipcshm.create(REQ_SHM_NAME, SHM_SIZE_REQ)
    if not ok then
        log("ipcshm.create failed for REQ: " .. tostring(err), 3)
        return false, err
    end
    req_shm = ok

    ok, err = ipcshm.create(RESP_SHM_NAME, SHM_SIZE_RESP)
    if not ok then
        log("ipcshm.create failed for RESP: " .. tostring(err), 3)
        return false, err
    end
    resp_shm = ok

    ok, err = ipcshm.create(CB_SHM_NAME, SHM_SIZE_CB)
    if not ok then
        log("ipcshm.create failed for CB: " .. tostring(err), 3)
        return false, err
    end
    cb_shm = ok

    -- Семафоры (open with initial 0 — ждём сигнала)
    ok, err = ipcsem.open(REQ_SEM_NAME, 1)
    if not ok then
        log("ipcsem.open failed for REQ: " .. tostring(err), 3)
        return false, err
    end
    req_sem = ok
   req_sem:dec()

    ok, err = ipcsem.open(RESP_SEM_NAME, 1)
    if not ok then
        log("ipcsem.open failed for RESP: " .. tostring(err), 3)
        return false, err
    end
    resp_sem = ok
    resp_sem:dec()

    ok, err = ipcsem.open(CB_SEM_NAME, 1)
    if not ok then
        log("ipcsem.open failed for CB: " .. tostring(err), 3)
        return false, err
    end
    cb_sem = ok
    cb_sem:dec()

    -- Мьютексы (open with initial 1 — unlocked)
    ok, err = ipcsem.open(REQ_MTX_NAME, 1)
    if not ok then
        log("ipcmtx.open failed for REQ: " .. tostring(err), 3)
        return false, err
    end
    req_mtx = ok

    ok, err = ipcsem.open(RESP_MTX_NAME, 1)
    if not ok then
        log("ipcmtx.open failed for RESP: " .. tostring(err), 3)
        return false, err
    end
    resp_mtx = ok

    ok, err = ipcsem.open(CB_MTX_NAME, 1)
    if not ok then
        log("ipcmtx.open failed for CB: " .. tostring(err), 3)
        return false, err
    end
    cb_mtx = ok

    -- Инициализируем заголовки (опционально, но для чистоты)
    req_shm:seek("set")
    req_shm:write(string.pack("<I4I4I4I4I4I4", MAGIC, VERSION, 0, 0, 0, 0))
    resp_shm:seek("set")
    resp_shm:write(string.pack("<I4I4I4I4I4I4", MAGIC, VERSION, 0, 0, 0, 0))
    cb_shm:seek("set")
    cb_shm:write(string.pack("<I4I4I4I4I4I4", MAGIC, VERSION, 0, 0, 0, 0))

    log("Shared memory IPC (Variant 2: separate buffers) initialized", 1)
    return true
end

function qsutils.connect(...)
    -- Игнорируем старые параметры сокетов (TCP)
    if is_connected then return true end
    local ok, err = init_shm()
    if not ok then
        log("IPC initialization failed: " .. tostring(err), 3)
        return false
    end
    is_connected = true
    log("QUIK# connected via shared memory (Variant 2)", 1)
    return true
end

-- Получение запроса от C#
function qsutils.receiveRequest(timeout_sec)
    if not is_connected then
        return nil, "not connected"
    end
    timeout_sec = timeout_sec or 5.0

    -- Ждём сигнала о новом запросе (C# > Lua)
    local success = req_sem:dec(timeout_sec)  -- dec/wait с таймаутом
    if not success then
        return nil, "timeout"
    end

    -- Захватываем мьютекс для чтения
    req_mtx:dec()

    req_shm:seek("set")
    local header = req_shm:read(HEADER_SIZE)
    if not header or #header < HEADER_SIZE then
        req_mtx:inc()
        return nil, "header read error"
    end
    local magic, ver, req_id, msg_type, body_len, _ = string.unpack("<I4I4I4I4I4I4", header)
    if magic ~= MAGIC then
        req_mtx:inc()
        return nil, "bad magic number"
    end

    -- Случай пустого тела — heartbeat или ping без данных
    if body_len == 0 then
        req_mtx:inc()
        return nil, "empty body", req_id
    end

    if body_len > SHM_SIZE_REQ - HEADER_SIZE then  -- Защита от overflow
        req_mtx:inc()
        return nil, "body too large: " .. body_len
    end

    req_shm:seek("set", HEADER_SIZE)
    local body = req_shm:read(body_len)
    if not body or #body ~= body_len then
        req_mtx:inc()
        return nil, "body read error"
    end

    local tbl, pos, err = json.decode(body, 1, json.null)
    if not tbl then
        req_mtx:inc()
        log("JSON decode failed: " .. tostring(err), 3)
        return nil, "json decode failed: " .. tostring(err)
    end

    -- Успех — освобождаем мьютекс и возвращаем
    req_mtx:inc()
    return tbl, req_id
end

-- Отправка ответа или колбэка в C#
local function send_message(msg_table, is_callback)
    local shm_to_use = is_callback and cb_shm or resp_shm
    local mtx_to_use = is_callback and cb_mtx or resp_mtx
    local sem_to_use = is_callback and cb_sem or resp_sem
    local shm_size = is_callback and SHM_SIZE_CB or SHM_SIZE_RESP

    if not is_connected then return nil, "not connected" end

    local str = to_json(msg_table)
    local len = #str
    if len > shm_size - HEADER_SIZE then
        return nil, "message too large: " .. len
    end

    -- Захватываем мьютекс для записи
    mtx_to_use:dec()

    -- Пишем тело
    shm_to_use:seek("set", HEADER_SIZE)
    shm_to_use:write(str)

    -- Пишем заголовок
    shm_to_use:seek("set")
    shm_to_use:write(string.pack("<I4I4I4I4I4I4",
        MAGIC, VERSION, msg_table.req_id or 0, 2, len, 0))  -- msg_type=2 для ответов/callbacks

    -- Освобождаем мьютекс
    mtx_to_use:inc()

    -- Сигнализируем о готовности
    local ok = sem_to_use:inc()
    if not ok then
        return nil, "sem inc failed"
    end

    log("Отправляемый JSON (длина " .. #str .. ", callback=" .. tostring(is_callback) .. "): " .. str, 1)
    return true
end

function qsutils.sendResponse(msg_table)
    return send_message(msg_table, false)  -- sync response
end

function qsutils.sendCallback(msg_table)
    return send_message(msg_table, true)  -- async callback
end

function qsutils.Close()
    -- Закрываем все ресурсы
    if req_shm then req_shm:close() end
    if resp_shm then resp_shm:close() end
    if cb_shm then cb_shm:close() end

    if req_sem then req_sem:close() end
    if resp_sem then resp_sem:close() end
    if cb_sem then cb_sem:close() end

    if req_mtx then req_mtx:close() end
    if resp_mtx then resp_mtx:close() end
    if cb_mtx then cb_mtx:close() end

    is_connected = false
    log("IPC closed (all resources released)", 1)
end

qsutils.is_connected = function() return is_connected end
sendResponse = qsutils.sendResponse
sendCallback = qsutils.sendCallback

return qsutils
--~ Copyright (c) 2014-2020 QUIKSharp Authors https://github.com/finsight/QUIKSharp/blob/master/AUTHORS.md. All rights reserved.
--~ Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.