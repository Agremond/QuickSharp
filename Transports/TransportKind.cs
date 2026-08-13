// Copyright (c) 2026 Your Name / QuikSharp Community
// Licensed under the Apache License, Version 2.0

using System;
using System.IO;
using System.Text.Json;

namespace QuikSharp.Transports
{
    /// <summary>
    /// Выбор транспорта QUIK# — shared memory (по умолчанию) или TCP ("netstack").
    /// </summary>
    public enum TransportKind
    {
        Shm,
        Tcp
    }

    /// <summary>
    /// Создаёт нужный <see cref="IQuikTransport"/>, в т.ч. по настройкам из lua/config.json —
    /// того же файла, который читает Lua-скрипт (см. lua/ipc.lua), чтобы выбор транспорта
    /// был общим для C# и Lua.
    /// </summary>
    public static class TransportFactory
    {
        public static IQuikTransport Create(TransportKind kind, string host = "127.0.0.1", int responsePort = 34130, int? callbackPort = null)
        {
            return kind == TransportKind.Tcp
                ? new TcpQuikTransport(host, responsePort, callbackPort)
                : new ShmQuikTransport();
        }

        /// <summary>
        /// Ищет lua/config.json, поднимаясь от каталога сборки вверх по дереву каталогов —
        /// lua/**/* не копируется в bin при сборке (Content без CopyToOutputDirectory), поэтому
        /// при запуске из bin/Debug/... нужно найти исходный lua/config.json в дереве проекта.
        /// Возвращает null, если файл не найден за 8 уровней.
        /// </summary>
        public static string? FindDefaultConfigPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "lua", "config.json");
                if (File.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        /// <summary>
        /// Как <see cref="ReadFromConfig(string, string)"/>, но сам находит lua/config.json
        /// через <see cref="FindDefaultConfigPath"/>.
        /// </summary>
        public static (TransportKind Kind, string Host, int ResponsePort, int CallbackPort) ReadFromDefaultConfig(
            string scriptName = "QuikSharp")
        {
            var path = FindDefaultConfigPath();
            return path == null
                ? (TransportKind.Shm, "127.0.0.1", 34130, 34131)
                : ReadFromConfig(path, scriptName);
        }

        /// <summary>
        /// Читает config.json (формат см. lua/config.json). При отсутствии файла, поля
        /// "transport" или записи для scriptName — возвращает (Shm, значения по умолчанию),
        /// т.е. поведение не меняется, если конфиг не трогали.
        /// </summary>
        public static (TransportKind Kind, string Host, int ResponsePort, int CallbackPort) ReadFromConfig(
            string configPath, string scriptName = "QuikSharp")
        {
            const string defaultHost = "127.0.0.1";
            const int defaultResponsePort = 34130;
            const int defaultCallbackPort = 34131;

            var result = (Kind: TransportKind.Shm, Host: defaultHost, ResponsePort: defaultResponsePort, CallbackPort: defaultCallbackPort);

            if (!File.Exists(configPath))
                return result;

            try
            {
                using var stream = File.OpenRead(configPath);
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;

                var kind = TransportKind.Shm;
                if (root.TryGetProperty("transport", out var transportProp) &&
                    transportProp.ValueKind == JsonValueKind.String &&
                    string.Equals(transportProp.GetString(), "tcp", StringComparison.OrdinalIgnoreCase))
                {
                    kind = TransportKind.Tcp;
                }

                string host = defaultHost;
                int responsePort = defaultResponsePort;
                int callbackPort = defaultCallbackPort;

                if (root.TryGetProperty("servers", out var servers) && servers.ValueKind == JsonValueKind.Array)
                {
                    foreach (var server in servers.EnumerateArray())
                    {
                        if (server.TryGetProperty("scriptName", out var nameProp) &&
                            string.Equals(nameProp.GetString(), scriptName, StringComparison.Ordinal))
                        {
                            if (server.TryGetProperty("responseHostname", out var h)) host = h.GetString() ?? host;
                            if (server.TryGetProperty("responsePort", out var rp)) responsePort = rp.GetInt32();
                            if (server.TryGetProperty("callbackPort", out var cp)) callbackPort = cp.GetInt32();
                            break;
                        }
                    }
                }

                return (kind, host, responsePort, callbackPort);
            }
            catch
            {
                return result;
            }
        }
    }
}
