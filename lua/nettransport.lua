-- nettransport.lua
-- IPC-модуль QUIK# на TCP ("netstack"): Lua выступает TCP-сервером (2 порта: response,
-- callback = response+1), C# подключается как TCP-клиент. Сообщения — JSON с разделителем "\n".
-- Альтернатива ShmQuikTransport/qsutils.lua, выбирается через lua/config.json ("transport").
--
-- Повторяет тот же контракт функций, что и qsutils.lua (connect/receiveRequest/sendResponse/
-- sendCallback/Close/is_connected), и те же глобальные хелперы
-- (log/delay/timemsec/split/split2/to_json/from_json/scriptFilename/paramsFromConfig),
-- так как qsfunctions.lua/qscallbacks.lua используют часть из них напрямую (не через qsutils),
-- поэтому этот модуль — полноценный самостоятельный "сосед" qsutils.lua, а не только IPC.

local socket = require "socket"
local json   = require "dkjson"

local qsutils = {}

--------------------------------------------------------------------------------
-- Вспомогательные функции времени и задержек (повторяет qsutils.lua)
--------------------------------------------------------------------------------

function delay(msec)
    if sleep then
        pcall(sleep, msec)
    else
        pcall(socket.sleep, msec / 1000)
    end
end

function timemsec()
    local st, res = pcall(socket.gettime)
    if st then
        return res * 1000
    else
        log("unexpected error in timemsec", 3)
        error("unexpected error in timemsec")
    end
end

function scriptFilename()
    if not debug or not debug.getinfo then
        return nil
    end
    local full_path = debug.getinfo(2, "S").source:sub(2)
    return full_path:match("[^\\]+%.lua[c]?$") or nil
end

--------------------------------------------------------------------------------
-- Пути и логирование
--------------------------------------------------------------------------------

local script_path = getScriptPath and getScriptPath() or "."

local logfile
local is_debug = true

local function openLog()
    os.execute('mkdir "' .. script_path .. '\\logs" 2>nul')
    local filename = script_path .. "\\logs\\QUIK#_" .. os.date("%Y%m%d") .. ".log"
    local f = io.open(filename, "a")
    if not f then
        f = io.open(filename, "a")
    end
    return f
end

local function closeLog()
    if logfile then
        pcall(logfile.close, logfile)
        logfile = nil
    end
end

function log(msg, level)
    msg = msg or ""
    level = level or 0

    local logLine = "LOG " .. level .. ": " .. msg

    print(logLine)

    if (level >= 1 and level <= 3) or is_debug then
        if message then
            pcall(message, msg, level)
        end
    end

    if logfile then
        local ms = math.floor(timemsec() % 1000)
        local timestamp = os.date("%Y-%m-%d %H:%M:%S") .. string.format(".%03d", ms)
        pcall(logfile.write, logfile, timestamp .. " " .. logLine .. "\n")
        pcall(logfile.flush, logfile)
    end
end

logfile = openLog()

--------------------------------------------------------------------------------
-- Конфиг и разбиение строк
--------------------------------------------------------------------------------

local function readConfigAsJson()
    local path = script_path .. "\\config.json"
    local f = io.open(path, "r")
    if not f then return nil end

    local content = f:read("*a")
    f:close()
    return from_json(content)
end

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

    return defaults
end

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

--------------------------------------------------------------------------------
-- JSON обёртки
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
-- IPC поверх TCP-сокетов (Lua = сервер, C# = клиент)
--------------------------------------------------------------------------------

local SCRIPT_NAME = "QuikSharp"   -- должно совпадать с записью "servers" в config.json

local response_server, callback_server
local response_client, callback_client
local is_connected_flag = false

--- неблокирующий accept на обоих серверных сокетах (settimeout(0) => accept() не блокирует)
local function try_accept_clients()
    if response_client and callback_client then
        return true
    end

    if not response_client then
        local ok, client = pcall(function() return response_server:accept() end)
        if ok and client then
            client:settimeout(0)
            pcall(client.setoption, client, "tcp-nodelay", true)
            response_client = client
            log("nettransport: response client connected", 1)
        end
    end

    if not callback_client then
        local ok, client = pcall(function() return callback_server:accept() end)
        if ok and client then
            client:settimeout(0)
            pcall(client.setoption, client, "tcp-nodelay", true)
            callback_client = client
            log("nettransport: callback client connected", 1)
        end
    end

    if response_client and callback_client then
        is_connected_flag = true
        log("QUIK# (tcp): client connected", 1)
        return true
    end

    return false
end

local function disconnect()
    is_connected_flag = false
    log("QUIK# (tcp): client disconnected", 1)
    if response_client then pcall(response_client.close, response_client); response_client = nil end
    if callback_client then pcall(callback_client.close, callback_client); callback_client = nil end
end

--- подключение/инициализация: биндит серверные сокеты (не блокирует)
function qsutils.connect()
    if response_server and callback_server then
        try_accept_clients()
        return true
    end

    local params = paramsFromConfig(SCRIPT_NAME)
    local host, response_port, callback_host, callback_port = params[1], params[2], params[3], params[4]

    local ok, err = pcall(function() response_server = assert(socket.bind(host, response_port, 1)) end)
    if not ok then
        log("nettransport: bind response server failed: " .. tostring(err), 3)
        return false
    end
    response_server:settimeout(0)

    ok, err = pcall(function() callback_server = assert(socket.bind(callback_host, callback_port, 1)) end)
    if not ok then
        log("nettransport: bind callback server failed: " .. tostring(err), 3)
        return false
    end
    callback_server:settimeout(0)

    log("QUIK# (tcp): listening on " .. host .. ":" .. response_port ..
        " (response) / " .. callback_host .. ":" .. callback_port .. " (callback)", 1)

    try_accept_clients()
    return true
end

--- читает запрос от C# (неблокирующе, с таймаутом), как qsutils.receiveRequest
function qsutils.receiveRequest(timeout_sec)
    timeout_sec = timeout_sec or 5.0

    if not response_server then
        return nil, "not connected"
    end

    if not is_connected_flag then
        try_accept_clients()
        if not is_connected_flag then
            return nil, "timeout"
        end
    end

    response_client:settimeout(timeout_sec)
    local ok, line, sockerr = pcall(response_client.receive, response_client, "*l")

    if ok and line then
        local tbl = from_json(line)
        if not tbl then
            return nil, "json decode failed"
        end
        return tbl, tbl.id or tbl.req_id
    end

    if ok and sockerr == "closed" then
        disconnect()
        return nil, "disconnected"
    end

    -- ok and sockerr == "timeout", либо pcall поймал реальную ошибку сокета
    return nil, "timeout"
end

--- отправка ответа или callback'а в C#
local function send_message(client, msg_table)
    if not client then
        return nil, "not connected"
    end

    local json_str = to_json(msg_table)
    client:settimeout(5)
    local ok, sent, sockerr = pcall(client.send, client, json_str .. "\n")

    if ok and sent then
        return true
    end

    if ok and sockerr == "closed" then
        disconnect()
    end

    return nil, (ok and sockerr) or "send failed"
end

function qsutils.sendResponse(msg_table)
    return send_message(response_client, msg_table)
end

function qsutils.sendCallback(msg_table)
    return send_message(callback_client, msg_table)
end

function qsutils.Close()
    if response_client then pcall(response_client.close, response_client) end
    if callback_client then pcall(callback_client.close, callback_client) end
    if response_server then pcall(response_server.close, response_server) end
    if callback_server then pcall(callback_server.close, callback_server) end

    response_client, callback_client = nil, nil
    response_server, callback_server = nil, nil
    is_connected_flag = false

    closeLog()
    log("nettransport: IPC (tcp) closed, all sockets released", 1)
end

qsutils.is_connected = function() return is_connected_flag end

-- для совместимости со старым кодом (qsfunctions.lua зовёт sendCallback как глобальную
-- функцию, например при публикации новой свечи из data source callback)
sendResponse = qsutils.sendResponse
sendCallback = qsutils.sendCallback

return qsutils

-- vim: ts=4 sts=4 sw=4 et
