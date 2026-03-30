using QuikSharp.DataStructures;
using QUIKSharp.DataStructures;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuikSharp
{
    /// <summary>
    /// Интерфейс сообщения для транспорта (System.Text.Json версия)
    /// </summary>
    internal interface IMessage
    {
        long? Id { get; set; }
        string cmd { get; set; }
        long CreatedTime { get; set; }
        DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Базовый класс сообщения
    /// </summary>
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
        public string cmd { get; set; }

        [JsonPropertyName("t")]
        public long CreatedTime { get; set; }

        [JsonPropertyName("v")]
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Универсальное сообщение с произвольными данными (полностью на System.Text.Json)
    /// </summary>
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

        /// <summary>
        /// Данные сообщения (может быть JsonElement после десериализации)
        /// </summary>
        [JsonPropertyName("data")]
        public object? Data { get; set; }

        /// <summary>
        /// Ошибка Lua (если есть)
        /// </summary>
        [JsonPropertyName("luaError")]
        public string? LuaError { get; set; }
        /// <summary>
        /// Универсальный метод получения данных в нужном типе (полностью STJ + улучшенная обработка)
        /// </summary>
        public T GetData<T>()
        {
            if (Data is T t)
                return t;

            if (Data is JsonElement je)
            {
                try
                {
                    var result = je.Deserialize<T>(JsonSerializerOptions);
                    return result ?? throw new JsonException($"Deserialization to {typeof(T).Name} returned null");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[GetData ERROR] {typeof(T).Name}: {ex.Message}");
                    Console.WriteLine($"Raw JSON: {je.GetRawText()}");   // используйте GetRawText()
                                                                         // Возвращаем default вместо падения всего транспорта
                    return default!;
                }
            }

            if (Data != null)
            {
                string json = JsonSerializer.Serialize(Data, JsonSerializerOptions);
                return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions)!;
            }

            throw new InvalidOperationException($"Data is null, cannot convert to {typeof(T).Name}");
        }

        /// <summary>
        /// Общие опции сериализации/десериализации (должны совпадать с теми, что используются в ShmQuikTransport)
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            //PropertyNameCaseInsensitive = true,
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // помогает частично
           
            Converters =
            {
               new StringToDecimalConverter(),
               new StringToIntConverter(),
               new EmptyStringToArrayConverter<OrderBook>()
            }
            // Можно добавить Converters при необходимости
        };
    }
}