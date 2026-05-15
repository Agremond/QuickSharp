local timeout = 1000  -- таймаут в миллисекундах

local is_run = true

-- Путь к QuikSharp
local quiksharp_path = "C:\\Programming\\QuikSharp\\lua\\QuikSharp.lua"

local v, r = getInfoParam("VERSION"):match("^(%d+%d*)%.(%d+%d*)")
if v * 1000 + r < 8006 then
    error("Для работы скрипта требуется версия 8.6 рабочего места Quik или новее", 3)
end

function OnStop()
    is_run = false
end

-- Проверка, запущен ли уже QuikSharp (через глобальную переменную)
local function isQuikSharpRunning()
    return _G.QuikSharpLoaded == true
end

function main()
    -- Однократная попытка запуска QuikSharp при старте
    if not isQuikSharpRunning() then
        local success, err = pcall(function()
            dofile(quiksharp_path)
        end)
        
        if success then
            message("QuikSharp startedd", 1)
            _G.QuikSharpLoaded = true  -- помечаем, что запущен
        else
            message("Error on start QuikSharp: " .. tostring(err), 3)
        end
    else
        message("QuikSharp started earlel ", 1)
    end

    while is_run do
        if isConnected() == 0 then
            sleep(timeout)
        end
        sleep(timeout)
    end
end