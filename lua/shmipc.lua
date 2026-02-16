-- shmipc.lua — IPC через shared memory для QUIKSharp (Windows-оптимизировано)
local shm = require "ipc.shm"
local sem = require "ipc.sem"
local dkjson = require "dkjson"

local M = {}

local SHM_NAME = "QuikSharpSHM"      -- 4 МБ
local SEM_TO_LUA = "QuikSharpToLua"   -- C# > Lua (запрос готов)
local SEM_TO_CS  = "QuikSharpToCS"    -- Lua > C# (ответ готов)

local SHM_SIZE = 4 * 1024 * 1024
local HEADER_SIZE = 20  -- magic(4) + ver(4) + req_id(4) + type(4) + len(4)

local mem, sem_to_lua, sem_to_cs
local is_connected = false

-- Инициализация (вызывается один раз)
function M.connect()
    if is_connected then return true end

    -- Создаём shared memory (Lua — owner)
    mem = assert(shm.create(SHM_NAME, SHM_SIZE), "Не удалось создать SHM")

    -- Создаём семафоры (начальное значение 0)
    sem_to_lua = assert(sem.open(SEM_TO_LUA, 0), "Не удалось создать семафор ToLua")
    sem_to_cs  = assert(sem.open(SEM_TO_CS,  0), "Не удалось создать семафор ToCS")

    -- Записываем заголовок
    mem:seek("set")
    mem:write(string.pack("I4I4I4I4I4", 0x5155494B, 1, 0, 0, 0))  -- QUIK magic

    is_connected = true
    print("QuikSharp: Shared Memory IPC инициализирован (4 МБ)")
    return true
end

-- Отправка сообщения C# > Lua (запрос)
function M.send_request(json_str, request_id)
    mem:seek("set", HEADER_SIZE)
    mem:write(json_str)
    
    -- Записываем заголовок
    mem:seek("set")
    mem:write(string.pack("I4I4I4I4I4", 
        0x5155494B,          -- magic
        1,                   -- version
        request_id or 0,
        1,                   -- type = 1 (request)
        #json_str
    ))

    sem_to_lua:inc()         -- сигналим C#, что запрос готов
end

-- Ожидание ответа от C#
function M.wait_response(timeout_sec)
    local got = sem_to_cs:dec(timeout_sec or 30)
    if not got then
        return nil, "timeout"
    end

    mem:seek("set")
    local header = mem:read(HEADER_SIZE)
    local magic, ver, req_id, msg_type, length = string.unpack("I4I4I4I4I4", header)

    if magic ~= 0x5155494B then
        return nil, "invalid magic"
    end

    mem:seek("set", HEADER_SIZE)
    local json_str = mem:read(length)
    return dkjson.decode(json_str), req_id
end

-- Получение запроса от C# (для callbacks и команд)
function M.wait_request(timeout_sec)
    local got = sem_to_lua:dec(timeout_sec or 5)
    if not got then return nil end

    mem:seek("set")
    local header = mem:read(HEADER_SIZE)
    local _, _, req_id, msg_type, length = string.unpack("I4I4I4I4I4", header)

    mem:seek("set", HEADER_SIZE)
    local json_str = mem:read(length)
    return dkjson.decode(json_str), req_id
end

-- Отправка ответа Lua > C#
function M.send_response(json_table, request_id)
    local json_str = dkjson.encode(json_table)
    
    mem:seek("set", HEADER_SIZE)
    mem:write(json_str)
    
    mem:seek("set")
    mem:write(string.pack("I4I4I4I4I4", 
        0x5157494B, 1, request_id or 0, 2, #json_str))  -- type = 2 (response)

    sem_to_cs:inc()
end

-- Закрытие
function M.close()
    if mem then mem:close() end
    if sem_to_lua then sem_to_lua:close() end
    if sem_to_cs then sem_to_cs:close() end
end

return M