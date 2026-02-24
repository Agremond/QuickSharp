-- qsutils.lua (исправленная версия)

local json = require "dkjson"
local shm  = require "ipc.shm"
local sem  = require "ipc.sem"

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
    time_offset = time_offset + 50   -- грубая эмуляция
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
    table.insert(params, 34130)       -- responsePort
    table.insert(params, "127.0.0.1") -- callbackHostname
    table.insert(params, 34131)       -- callbackPort

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
-- ТРАНСПОРТ: shared memory + семафоры
-- =============================================================================

local SHM_NAME    = "QuikSharp_SHM_v2"
local SEM_CS2LUA  = "QuikSharp_CS2Lua"   -- C# > Lua (запрос готов)
local SEM_LUA2CS  = "QuikSharp_Lua2CS"   -- Lua > C# (ответ готов)

local shm_handle
local sem_cs2lua
local sem_lua2cs
local is_connected = false

local HEADER_SIZE = 24
local MAGIC = 0x5155494B   -- "QUIK"

local function init_shm()
    if shm_handle then return true end

    local ok, err = shm.create(SHM_NAME, 4*1024*1024)
    if not ok then
        log("shm.create failed: " .. tostring(err), 3)
        return false, err
    end
    shm_handle = ok

    ok, err = sem.open(SEM_CS2LUA, 1)
    if not ok then
        log("sem.open CS2Lua failed: " .. tostring(err), 3)
        return false, err
    end
    sem_cs2lua = ok

    ok, err = sem.open(SEM_LUA2CS, 1)
    if not ok then
        log("sem.open Lua2CS failed: " .. tostring(err), 3)
        return false, err
    end
    sem_lua2cs = ok

    shm_handle:seek("set")
    shm_handle:write(string.pack("<I4I4I4I4I4I4", MAGIC, 2, 0, 0, 0, 0))

    log("Shared memory IPC initialized", 1)
    return true
end

function qsutils.connect(...)
    -- игнорируем старые параметры сокетов
    if is_connected then return true end

    local ok, err = init_shm()
    if not ok then
        log("IPC initialization failed: " .. tostring(err), 3)
        return false
    end

    is_connected = true
    log("QUIK# connected via shared memory", 1)
    return true
end
-- Получение запроса от C#
function qsutils.receiveRequest(timeout_sec)
    if not is_connected then
        return nil, "not connected"
    end

    timeout_sec = timeout_sec or 5.0

    -- ждём сигнала от семафора (C# > Lua)
    local success = sem_cs2lua:dec(timeout_sec)
    if not success then
        return nil, "timeout"
    end

    shm_handle:seek("set")
    local header = shm_handle:read(HEADER_SIZE)
    if not header or #header < HEADER_SIZE then
        sem_lua2cs:inc()   -- освобождаем семафор, чтобы не заблокировать очередь
        return nil, "header read error"
    end

    local magic, ver, req_id, msg_type, body_len, _ = string.unpack("<I4I4I4I4I4I4", header)

    if magic ~= MAGIC then
        sem_lua2cs:inc()
        return nil, "bad magic number"
    end

    -- ------------------------------------------------
    -- Случай пустого тела — это нормальная ситуация
    -- (heartbeat, ping без данных, некоторые команды)
    -- ------------------------------------------------
    if body_len == 0 then
        -- Мы НЕ инкрементим семафор здесь — это делает вызывающий код
        return nil, "empty body", req_id
    end

    if body_len > 4*1024*1024 then   -- защита от слишком больших сообщений
        sem_lua2cs:inc()
        return nil, "body too large: " .. body_len
    end

    shm_handle:seek("set", HEADER_SIZE)
    local body = shm_handle:read(body_len)
    if not body or #body ~= body_len then
        sem_lua2cs:inc()
        return nil, "body read error"
    end

    local tbl, pos, err = json.decode(body, 1, json.null)
    if not tbl then
        sem_lua2cs:inc()
        log("JSON decode failed: " .. tostring(err), 3)
        return nil, "json decode failed: " .. tostring(err)
    end

    -- Успех — возвращаем распарсенный объект и req_id
    return tbl, req_id
end

-- Отправка ответа или колбэка в C#
local function send_message(msg_table, msg_type)
    if not is_connected then return nil, "not connected" end

    local str = json.encode(msg_table)
    local len = #str

    shm_handle:seek("set", HEADER_SIZE)
    shm_handle:write(str)

    shm_handle:seek("set")
    shm_handle:write(string.pack("<I4I4I4I4I4I4",
        MAGIC, 2, msg_table.req_id or 0, msg_type, len, 0))

    local ok = sem_lua2cs:inc()
    if not ok then
        return nil, "sem inc failed"
    end

    return true
end

function qsutils.sendResponse(msg_table)
    return send_message(msg_table, 2)
end

function qsutils.sendCallback(msg_table)
    return send_message(msg_table, 2)   -- пока одинаково
end

function qsutils.Close()
    if shm_handle then shm_handle:close() end
    if sem_cs2lua then sem_cs2lua:close() end
    if sem_lua2cs then sem_lua2cs:close() end
    is_connected = false
    log("IPC closed", 1)
end

qsutils.is_connected = function() return is_connected end

return qsutils