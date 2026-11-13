-- Второй экземпляр QUIK# — для сборщика истории OptionTerminal (OptionTerminal.exe --collector).
-- Порты берутся из config.json, сервер "Quik_2" (34132/34133); основной QuikSharp.lua остаётся
-- на 34130/34131 для OptionTerminal. Запускать в QUIK: Сервисы -> Lua скрипты -> Добавить Quik_2.lua.
QUIKSHARP_SCRIPT_NAME = "Quik_2"
dofile(getScriptPath() .. "\\QuikSharp.lua")
