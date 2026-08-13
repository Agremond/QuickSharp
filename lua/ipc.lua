-- ipc.lua
-- Точка выбора транспорта QUIK#: читает "transport" ("shm"|"tcp", по умолчанию "shm")
-- из верхнего уровня lua/config.json и требует ровно один из qsutils (shm) / nettransport
-- (tcp) — никогда оба сразу. QuikSharp.lua и qscallbacks.lua требуют этот модуль вместо
-- прямого "qsutils", чтобы оба видели один и тот же выбранный транспорт.

local function read_transport_kind()
    local script_path = getScriptPath and getScriptPath() or "."
    local f = io.open(script_path .. "\\config.json", "r")
    if not f then return "shm" end

    local content = f:read("*a")
    f:close()

    local json = require "dkjson"
    local ok, cfg = pcall(json.decode, content, 1, json.null)
    if ok and cfg and type(cfg.transport) == "string" then
        return cfg.transport:lower()
    end

    return "shm"
end

if read_transport_kind() == "tcp" then
    return require "nettransport"
else
    return require "qsutils"
end
