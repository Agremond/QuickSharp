-- qsutils.lua
-- Утилиты для QUIK# (взаимодействие Lua ↔ C# через разделяемую память)

local json   = require "dkjson"
local ipcshm = require "ipc.shm"   -- работа с shared memory
local ipcsem = require "ipc.sem"   -- работа с семафорами

local qsutils = {}

--------------------------------------------------------------------------------
-- Вспомогательные функции времени и задержки
--------------------------------------------------------------------------------

--- Кроссплатформенная задержка в миллисекундах
function delay(msec)
    if sleep then
        pcall(sleep, msec)
    else
        -- socket.sleep(msec / 1000)   -- закомментировано, т.к. socket может отсутствовать
    end
end

--- Миллисекундное время с монотонным приращением (защита от скачков os.time)
local time_offset = 0
local last_os_time = os.time()

function timemsec()
    local now = os.time()
    if now > last_os_time then
        time_offset = 0
        last_os_time = now
    end
    time_offset = time_offset + 50          -- грубое приращение ~50 мс
    return (now * 1000) + (time_offset % 1000)
end

--------------------------------------------------------------------------------
-- Работа с путями и именем скрипта
--------------------------------------------------------------------------------

local script_path = getScriptPath and getScriptPath() or "."

--- Имя текущего скрипта без пути и расширения
function scriptFilename()
    if not debug or not debug.getinfo then
        return nil
    end
    local full_path = debug.getinfo(2, "S").source:sub(2)
    return full_path:match("[^\\]+%.lua[c]?$") or nil
end

--------------------------------------------------------------------------------
-- Логирование
--------------------------------------------------------------------------------

local logfile
local is_debug = false

--- Создаёт папку logs и открывает лог-файл на текущий день
local function openLog()
    os.execute('mkdir "' .. script_path .. '\\logs" 2>nul')
    local filename = script_path .. "\\logs\\QUIK#_" .. os.date("%Y%m%d") .. ".log"
    local f = io.open(filename, "a")
    if not f then
        -- повторная попытка (иногда помогает)
        f = io.open(filename, "a")
    end
    return f
end

--- Закрывает лог-файл, если он открыт
local function closeLog()
    if logfile then
        pcall(logfile.close, logfile)
        logfile = nil
    end
end

--- Основная функция логирования (в файл + в окно сообщений QUIK)
function log(msg, level)
    msg = msg or ""
    level = level or 0

    local logLine = "LOG " .. level .. ": " .. msg

    -- Вывод в консоль
    print(logLine)

    -- Вывод в окно сообщений QUIK (только важные уровни или при дебаге)
    if (level >= 1 and level <= 3) or is_debug then
        if message then
            pcall(message, msg, level)
        end
    end

    -- Запись в файл
    if logfile then
        local ms = math.floor(timemsec() % 1000)
        local timestamp = os.date("%Y-%m-%d %H:%M:%S") .. string.format(".%03d", ms)
        pcall(logfile.write, logfile, timestamp .. " " .. logLine .. "\n")
        pcall(logfile.flush, logfile)
    end
end

logfile = openLog()

--------------------------------------------------------------------------------
-- Работа с конфигурацией
--------------------------------------------------------------------------------

--- Читает config.json и возвращает таблицу или nil
local function readConfigAsJson()
    local path = script_path .. "\\config.json"
    local f = io.open(path, "r")
    if not f then return nil end

    local content = f:read("*a")
    f:close()
    return from_json(content)
end

--- Возвращает параметры подключения для указанного скрипта из config.json
function paramsFromConfig(scriptName)
    local defaults = {
        "127.0.0.1",   -- responseHostname
        34130,         -- responsePort
        "127.0.0.1",   -- callbackHostname
        34131          -- callbackPort
    }

    local config = readConfigAsJson()
    if not config or not config.servers then
        return defaults
    end

    for _, server in ipairs(config.servers) do
        if server.scriptName == scriptName then
            if server.responseHostname then defaults[1] = server.responseHostname end
            if server.responsePort     then defaults[2] = server.responsePort     end
            if server.callbackHostname then defaults[3] = server.callbackHostname end
            if server.callbackPort     then defaults[4] = server.callbackPort     end
            return defaults
        end
    end

    return defaults   -- если ничего не нашли — возвращаем значения по умолчанию
end

--------------------------------------------------------------------------------
-- JSON утилиты
--------------------------------------------------------------------------------

function from_json(str)
    local ok, result = pcall(json.decode, str, 1, json.null)
    if ok then
        return result
    else
        log("JSON decode error: " .. tostring(result), 3)
        return nil
    end
end

function to_json(tbl)
    local ok, result = pcall(json.encode, tbl, { indent = false })
    if ok then
        return result
    else
        error("JSON encode failed: " .. tostring(result))
    end
end

--------------------------------------------------------------------------------
-- IPC через разделяемую память (вариант 2 — отдельные буферы)
--------------------------------------------------------------------------------

-- Имена объектов IPC
local REQ_SHM_NAME  = "QuikSharp_Request_Shmem"
local RESP_SHM_NAME = "QuikSharp_Response_Shmem"
local CB_SHM_NAME   = "QuikSharp_Callback_Shmem"

local REQ_SEM_NAME  = "QuikSharp_Request_Sem"
local RESP_SEM_NAME = "QuikSharp_Response_Sem"
local CB_SEM_NAME   = "QuikSharp_Callback_Sem"

local REQ_MTX_NAME  = "QuikSharp_Request_MutexSem"
local RESP_MTX_NAME = "QuikSharp_Response_MutexSem"
local CB_MTX_NAME   = "QuikSharp_Callback_MutexSem"

-- Размеры памяти
local SHM_SIZE_REQ = 1024 * 1024      -- 1 MB
local SHM_SIZE_RESP = 1024 * 1024     -- 1 MB
local SHM_SIZE_CB  = 2 * 1024 * 1024  -- 2 MB

local HEADER_SIZE = 24
local MAGIC       = 0x5155494B        -- "QUIK"
local VERSION     = 2

local req_shm, resp_shm, cb_shm
local req_sem, resp_sem, cb_sem
local req_mtx, resp_mtx, cb_mtx

local is_connected = false

--- Инициализация всех объектов разделяемой памяти и синхронизации
local function init_shm()
    if req_shm then return true end

    -- Создание shared memory
    local ok, err

    ok, err = ipcshm.create(REQ_SHM_NAME, SHM_SIZE_REQ)
    if not ok then log("ipcshm.create REQ failed: " .. tostring(err), 3); return false end
    req_shm = ok

    ok, err = ipcshm.create(RESP_SHM_NAME, SHM_SIZE_RESP)
    if not ok then log("ipcshm.create RESP failed: " .. tostring(err), 3); return false end
    resp_shm = ok

    ok, err = ipcshm.create(CB_SHM_NAME, SHM_SIZE_CB)
    if not ok then log("ipcshm.create CB failed: " .. tostring(err), 3); return false end
    cb_shm = ok

    -- Семафоры событий (счётчики)
    ok, err = ipcsem.open(REQ_SEM_NAME, 1);  if not ok then return false end; req_sem  = ok; req_sem:dec()
    ok, err = ipcsem.open(RESP_SEM_NAME, 1); if not ok then return false end; resp_sem = ok; resp_sem:dec()
    ok, err = ipcsem.open(CB_SEM_NAME, 1);   if not ok then return false end; cb_sem   = ok; cb_sem:dec()

    -- Мьютексы (взаимное исключение)
    ok, err = ipcsem.open(REQ_MTX_NAME, 1);  if not ok then return false end; req_mtx  = ok
    ok, err = ipcsem.open(RESP_MTX_NAME, 1); if not ok then return false end; resp_mtx = ok
    ok, err = ipcsem.open(CB_MTX_NAME, 1);   if not ok then return false end; cb_mtx   = ok

    -- Инициализация заголовков в памяти
    local header = string.pack("<I4I4I4I4I4I4", MAGIC, VERSION, 0, 0, 0, 0)
    req_shm:seek("set");  req_shm:write(header)
    resp_shm:seek("set"); resp_shm:write(header)
    cb_shm:seek("set");   cb_shm:write(header)

    log("Shared memory IPC (variant 2) initialized", 1)
    return true
end

--------------------------------------------------------------------------------
-- Основные публичные функции
--------------------------------------------------------------------------------

function qsutils.connect()
    if is_connected then return true end

    local ok, err = init_shm()
    if not ok then
        log("IPC initialization failed: " .. tostring(err), 3)
        return false
    end

    is_connected = true
    log("QUIK# connected via shared memory (variant 2)", 1)
    return true
end

--- Чтение запроса от C# (блокирующее с таймаутом)
function qsutils.receiveRequest(timeout_sec)
    if not is_connected then return nil, "not connected" end
    timeout_sec = timeout_sec or 5.0

    local ok = req_sem:dec(timeout_sec)
    if not ok then return nil, "timeout" end

    req_mtx:dec()

    req_shm:seek("set")
    local header = req_shm:read(HEADER_SIZE)
    if not header or #header < HEADER_SIZE then
        req_mtx:inc()
        return nil, "header read error"
    end

    local magic, ver, req_id, msg_type, body_len = string.unpack("<I4I4I4I4I4", header)
    if magic ~= MAGIC then
        req_mtx:inc()
        return nil, "bad magic number"
    end

    if body_len == 0 then
        req_mtx:inc()
        return nil, "empty body", req_id
    end

    if body_len > SHM_SIZE_REQ - HEADER_SIZE then
        req_mtx:inc()
        return nil, "body too large"
    end

    req_shm:seek("set", HEADER_SIZE)
    local body = req_shm:read(body_len)
    req_mtx:inc()

    if not body or #body ~= body_len then
        return nil, "body read error"
    end

    local tbl = from_json(body)
    if not tbl then
        return nil, "json decode failed"
    end

    return tbl, req_id
end

--- Отправка ответа или callback'а в C#
local function send_message(msg_table, is_callback)
    if not is_connected then return nil, "not connected" end

    local shm = is_callback and cb_shm or resp_shm
    local mtx = is_callback and cb_mtx or resp_mtx
    local sem = is_callback and cb_sem or resp_sem
    local max_size = is_callback and SHM_SIZE_CB or SHM_SIZE_RESP

    local json_str = to_json(msg_table)
    local len = #json_str

    if len > max_size - HEADER_SIZE then
        return nil, "message too large (" .. len .. " bytes)"
    end

    mtx:dec()

    shm:seek("set", HEADER_SIZE)
    shm:write(json_str)

    shm:seek("set")
    shm:write(string.pack("<I4I4I4I4I4I4",
        MAGIC, VERSION, msg_table.req_id or 0, 2, len, 0))

    mtx:inc()

    local ok = sem:inc()
    if not ok then
        return nil, "semaphore increment failed"
    end

    return true
end

function qsutils.sendResponse(msg_table)
    return send_message(msg_table, false)
end

function qsutils.sendCallback(msg_table)
    return send_message(msg_table, true)
end

function qsutils.Close()
    if req_shm  then req_shm:close()  end
    if resp_shm then resp_shm:close() end
    if cb_shm   then cb_shm:close()   end

    if req_sem  then req_sem:close()  end
    if resp_sem then resp_sem:close() end
    if cb_sem   then cb_sem:close()   end

    if req_mtx  then req_mtx:close()  end
    if resp_mtx then resp_mtx:close() end
    if cb_mtx   then cb_mtx:close()   end

    closeLog()
    is_connected = false
    log("IPC closed, all resources released", 1)
end

qsutils.is_connected = function() return is_connected end

-- Для совместимости со старым кодом
sendResponse = qsutils.sendResponse
sendCallback = qsutils.sendCallback

return qsutils

-- vim: ts=4 sts=4 sw=4 et