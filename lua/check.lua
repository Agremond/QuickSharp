
-- или если у тебя dll называется ipc.dll и лежит рядом
package.cpath = package.cpath .. ";./?.dll"
local shm = require("ipc.shm")
local sem = require("ipc.sem")
if not sem then
    error("ipc.sem не загрузился! Проверь:")
    print("1. dll содержит sem.c и скомпилирована правильно?")
    print("2. package.cpath включает путь к ipc.dll / ipc_sem.dll?")
    print("3. require(\"ipc.sem\") > ", sem)
    return
end
print("sem загружен: ", type(sem))          -- должно быть table
print("sem.create: ", type(sem.open))     -- function
if type(shm) ~= "table" or type(shm.create) ~= "function" then
    print("Модуль ipc.shm не загрузился корректно")
    return
end
if type(sem) ~= "table" or type(sem.open) ~= "function" then
    print("Модуль ipc.shm не загрузился корректно")
    return
end

-- Проверка, что получилось
print(type(shm))          -- должно быть table
print(shm.create)         -- должна быть function


local name = "MyQuikChannel"   -- Global\\ — важно для доступа из других сессий/пользователей
local size = 65536                     -- 64 КБ, подбери под свои нужды
local writer_sem = sem.open("QuikDataReady", 0)
-- Создаём (или пытаемся, если уже есть — выдаст ошибку)
local handle = shm.create(name, size)

if not handle then
    -- Если уже существует — просто подключаемся
    handle = shm.attach(name)
    if not handle then
        print("Error connection to " .. name)
        return
    end
    print("Connection ON")
else
    print("Create new segment " .. name .. ", size " .. handle:size() .. " byte")
end

-- Пример: пишем строку в начало
local data = "Hello from QUIK! Time: " .. os.date("%H:%M:%S")
local written = handle:write(data)   -- пишет строку как есть
-- writer_sem:close()
if written then
    print("Writed: " .. data)
else
    print("Write error")
end

-- Читаем обратно для проверки (смещаемся в начало)
handle:seek("set", 0)
local read_data = handle:read(#data)
print("Readed: " .. (read_data or "<nil>"))

-- Не забываем закрывать (хотя GC сам закроет при выходе)
-- handle:close()   -- опционально