-- ipc_quik.lua
-- Обёртка над lua-luaipc для QUIKSharp (два канала: запросы и ответы)

local M = {}

local shm = require("ipc.shm")
local sem = require("ipc.sem")

-- Имена (должны совпадать в C# !!!)
M.REQ_SHM_NAME   = "QuikSharp_Requests"     -- C# -> Lua (запросы)
M.RESP_SHM_NAME  = "QuikSharp_Responses"    -- Lua -> C# (ответы + коллбэки)
M.SEM_NEW_REQ    = "QuikSharp_NewRequest"   -- сигнал о новом запросе
M.SEM_NEW_RESP   = "QuikSharp_NewResponse"  -- сигнал о новом ответе/коллбэке

-- Размер сегментов (подбери под максимальный размер сообщений + запас)
M.SHM_SIZE = 1024 * 1024  -- 1 МБ — хватит для большинства случаев

-- Инициализация (вызывается один раз при старте)
function M.init()
    if M.initialized then return true end

    -- Запросы (C# пишет > Lua читает)
    M.req_handle = shm.attach(M.REQ_SHM_NAME)
    if not M.req_handle then
        M.req_handle = shm.create(M.REQ_SHM_NAME, M.SHM_SIZE)
        if not M.req_handle then
            print("ipc_quik: Не удалось создать/подключить " .. M.REQ_SHM_NAME)
            return false
        end
        print("ipc_quik: Создан новый сегмент запросов " .. M.REQ_SHM_NAME)
    else
        print("ipc_quik: Подключен существующий сегмент запросов")
    end

    -- Ответы (Lua пишет > C# читает)
    M.resp_handle = shm.attach(M.RESP_SHM_NAME)
    if not M.resp_handle then
        M.resp_handle = shm.create(M.RESP_SHM_NAME, M.SHM_SIZE)
        if not M.resp_handle then
            print("ipc_quik: Не удалось создать/подключить " .. M.RESP_SHM_NAME)
            return false
        end
        print("ipc_quik: Создан новый сегмент ответов " .. M.RESP_SHM_NAME)
    else
        print("ipc_quik: Подключен существующий сегмент ответов")
    end

    -- Семафоры уведомлений
    M.sem_new_req = sem.open(M.SEM_NEW_REQ)
    if not M.sem_new_req then
        M.sem_new_req = sem.create(M.SEM_NEW_REQ, 0)
        if not M.sem_new_req then
            print("ipc_quik: Не удалось создать семафор " .. M.SEM_NEW_REQ)
            return false
        end
    end

    M.sem_new_resp = sem.open(M.SEM_NEW_RESP)
    if not M.sem_new_resp then
        M.sem_new_resp = sem.create(M.SEM_NEW_RESP, 0)
        if not M.sem_new_resp then
            print("ipc_quik: Не удалось создать семафор " .. M.SEM_NEW_RESP)
            return false
        end
    end

    M.initialized = true
    print("ipc_quik: Инициализация завершена успешно")
    return true
end

-- Простая отправка строки (ответ / коллбэк) > перезапись с начала
-- В production лучше добавить ring-buffer или заголовок (длина + данные)
function M.send_response(data)  -- data — строка JSON + \n
    if not M.initialized then return false end

    -- Пишем с начала сегмента (простой вариант)
    local ok = M.resp_handle:write(data, 0)  -- позиция 0
    if ok then
        M.sem_new_resp:post()   -- уведомляем C#
        return true
    else
        print("ipc_quik: Ошибка записи в ответный сегмент")
        return false
    end
end

-- Чтение запроса (блокирует до появления данных)
-- Возвращает строку или nil + ошибка
function M.receive_request(timeout_ms)
    if not M.initialized then return nil, "not initialized" end

    local ok = M.sem_new_req:wait(timeout_ms or 5000)  -- ждём сигнал от C#
    if not ok then
        return nil, "timeout or error waiting for new request"
    end

    -- Читаем с начала (простой вариант — предполагаем, что одно сообщение)
    M.req_handle:seek("set", 0)
    local data = M.req_handle:read(M.SHM_SIZE)  -- читаем много, потом обрежем

    if data and #data > 0 then
        -- Убираем trailing \0 если есть
        data = data:gsub("%z.*$", "")
        return data
    end

    return nil, "empty or error reading request"
end

-- Очистка после чтения (опционально, чтобы C# знал, что обработано)
function M.clear_request()
    if M.req_handle then
        M.req_handle:write(string.char(0), 0)  -- маркер пустоты
    end
end

return M