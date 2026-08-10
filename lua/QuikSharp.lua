-- QuikSharp.lua
-- ������� ������ QUIK# � �������������� shared memory (ipc.shm + ipc.sem)
-- ���������� qsutils.lua (������ � connect / receiveRequest / sendResponse)

local util = require "qsutils"
local json = require "dkjson"           -- ���� ����� �������

-- ����������� ������� � ������� QUIK (���� ����� ����������)
local qf = require "qsfunctions"        -- ��������� ������
local callbacks = require "qscallbacks" -- ��������� �������

-- ��������, ������� �� ������ � QUIK
function is_quik()
    return getScriptPath ~= nil
end

quikVersion = nil
script_path = "."

if is_quik() then
    script_path = getScriptPath()
    
    quikVersion = getInfoParam("VERSION")
    if quikVersion then
        local t = {}
        for str in string.gmatch(quikVersion, "([^%.]+)") do
            table.insert(t, str)
        end
        quikVersion = tonumber(t[1]) * 100 + tonumber(t[2])
    end
    
    if quikVersion == nil then
        message("QUIK# cannot detect QUIK version", 3)
        return
    end
    
    local linkage = "MD"
    local libPath
    
    if quikVersion >= 805 then
        libPath = "\\clibs64\\53_" .. linkage .. "\\"
    elseif quikVersion >= 800 then
        libPath = "\\clibs64\\5.1_" .. linkage .. "\\"
    else
        libPath = "\\clibs\\5.1_" .. linkage .. "\\"
    end
    
    package.path  = package.path  .. ";" .. script_path .. "\\?.lua;" .. script_path .. "\\?.luac"
    package.cpath = package.cpath .. ";" .. script_path .. libPath .. "?.dll;" .. "." .. libPath .. "?.dll"
end

log("Detected Quik version: " .. (quikVersion or "unknown") .. ", script path: " .. script_path, 0)

-- �������� ������� ���������� �������
function IsScriptRunning()
    return getScriptPath() ~= nil
end

--- ������� ������� (QUIK �������� �������������)
function main()
    message("QuikSharp: ������...", 1)

    local connected = util.connect()
    if not connected then
        message("QuikSharp: �� ������� ���������������� shared memory", 3)
        return
    end

    message("QuikSharp: IPC (shared memory) ������� ���������������", 1)

    while IsScriptRunning() do
        local cmd, req_id, err = util.receiveRequest(0.150)   -- 50 �� � �������� ������

        if cmd then
		
            -- --------------------------------
            -- ��������� ������ �� C#
            -- --------------------------------
            --log("������ �� C# 1 (req_id=" .. tostring(req_id).."): " .. to_json(cmd), 0)
	    if cmd.cmd == "ping" then
            -- ����������� ��������� ping � ������� ���-������
            response = {
                cmd    = "ping",
                req_id = req_id,          -- ����������� ���������� ��� �� id
                data   = "Pong",          -- ��� "Ping", "OK", "" � �����������
                t      = timemsec(),
                success = true
            }
        else	
	        local result = qf.dispatch_and_process(cmd)
		if cmd.nonce then
		    result.nonce = cmd.nonce   -- ��������� ������� � �����
        end
		result.req_id = req_id
		--log("����� dispatch: cmd=" .. tostring(result.cmd) .. ", data ���=" .. type(result.data), 1)
	        local ok, send_err = util.sendResponse(result)
	        if not ok then
	            log("������ �������� ������: " .. tostring(send_err), 2)
	        end
	   end

        elseif err == "timeout" then
            -- ������, ���� � ������ ��� ������ ����
            sleep(5)

        elseif err == "empty body" then
            -- --------------------------------
            -- ������ ��������� � ����� ��������� pong / heartbeat
            -- --------------------------------
            local response = {
                cmd     = "heartbeat",
                req_id  = req_id,
                t       = timemsec(),
                success = true
            }
            local ok, send_err = util.sendResponse(response)
            if not ok then
                log("������ �������� heartbeat: " .. tostring(send_err), 2)
            end

        else
            -- --------------------------------
            -- ������ ������ / ������� / �����
            -- --------------------------------
            if err then
                log("������ ����� �������: " .. tostring(err), 2)
            end
            sleep(10)   -- ��������� ����� ����� ��������� ��������
        end
    end

    util.Close()
    message("QuikSharp: ������ ���������", 1)
end

-- ������������ QUIK-�������
function OnInit()
    -- ����� �������� �������������� �������������, ���� �����
end

function OnStop()
    util.Close()
    message("QuikSharp: OnStop > IPC ������", 1)
end

-- OnOrder/OnTrade здесь раньше переопределялись, но проверяли callbacks.OnOrder/callbacks.OnTrade,
-- а qscallbacks.lua возвращает пустую таблицу (все функции там — глобальные, не поля таблицы) —
-- условие всегда было false, и util.sendCallback никогда не вызывался. Убрано: глобальные
-- OnOrder/OnTrade/OnTransReply/OnQuote/OnFuturesClientHolding/..., объявленные внутри
-- qscallbacks.lua (require выше), и так корректно шлют sendEvent(...) сами, без обёрток здесь.

message("QuikSharp ����� � ����� � ������ (SHM-�����)", 1)