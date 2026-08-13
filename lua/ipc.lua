-- ipc.lua
-- ВНИМАНИЕ: этот файл должен сохраняться в кодировке Windows-1251 (cp1251), НЕ в UTF-8.
-- Lua-окружение QUIK читает и показывает кириллицу (в message()/log() и в самих
-- исходниках) только как cp1251 -- сохранение в UTF-8 превращает все русские строки
-- и комментарии в нечитаемый мусор (см. lua/QuikSharp.lua и его git-историю, где это
-- уже однажды случилось).
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
