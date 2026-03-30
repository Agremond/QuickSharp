using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuikSharp
{
    internal interface IMessage
    {
        long? Id { get; set; }
        string cmd { get; set; }
        long CreatedTime { get; set; }
        DateTime? ValidUntil { get; set; }
    }

    internal abstract class BaseMessage : IMessage
    {
        protected static readonly long Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000L;

        protected BaseMessage(string command = "", DateTime? validUntil = null)
        {
            cmd = command;
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ValidUntil = validUntil;
        }

        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("cmd")]
        public string cmd { get; set; } = string.Empty;

        [JsonPropertyName("t")]
        public long CreatedTime { get; set; }

        [JsonPropertyName("v")]
        public DateTime? ValidUntil { get; set; }
    }

    internal class Message : BaseMessage
    {
        public Message() { }

        public Message(object? data, string command, DateTime? validUntil = null)
        {
            cmd = command;
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ValidUntil = validUntil;
            Data = data;
        }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("luaError")]
        public string? LuaError { get; set; }

        /// <summary>
        /// Улучшенный GetData<T> — безопасная десериализация
        /// </summary>
        public T? GetData<T>()
        {
            if (Data is T t)
                return t;

            if (Data is JsonElement je)
            {
                try
                {
                    return je.Deserialize<T>(QuikJson.Options)
                        ?? throw new JsonException($"Deserialize to {typeof(T).Name} returned null");
                }
                catch (JsonException ex)
                {
                    string raw = je.GetRawText();
                    Console.WriteLine($"[GetData ERROR] {typeof(T).Name}: {ex.Message}");
                    Console.WriteLine($"Raw JSON: {raw}");
                    // Можно дополнительно бросить кастомное исключение, если нужно
                    throw new JsonException($"Failed to deserialize {typeof(T).Name}. Raw: {raw}", ex);
                }
            }

            if (Data != null)
            {
                try
                {
                    string json = JsonSerializer.Serialize(Data, QuikJson.Options);
                    return JsonSerializer.Deserialize<T>(json, QuikJson.Options);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[GetData ERROR] {typeof(T).Name} (fallback): {ex.Message}");
                    throw;
                }
            }

            return default;
        }
    }
}