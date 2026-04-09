using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuikSharp
{
    /// <summary>
    /// Lightweight JSON helpers (адаптировано под System.Text.Json + SHM)
    /// </summary>
    public static class JsonExtensions
    {
        // Глобальные настройки, которые вы уже используете в ShmQuikTransport
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            WriteIndented = false,
            // Можно добавить другие опции по необходимости
        };

        [ThreadStatic]
        private static StringBuilder? _stringBuilder;

        /// <summary>Десериализация из строки</summary>
        public static T? FromJson<T>(this string json)
            => JsonSerializer.Deserialize<T>(json, DefaultOptions);

        /// <summary>Десериализация из строки в указанный тип</summary>
        public static object? FromJson(this string json, Type type)
            => JsonSerializer.Deserialize(json, type, DefaultOptions);

        /// <summary>Сериализация в компактный JSON</summary>
        public static string ToJson<T>(this T? obj)
        {
            if (obj == null)
                return "null";

            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder(8192);

            _stringBuilder.Clear();

            // Используем ArrayBufferWriter для минимальных аллокаций
            var bufferWriter = new ArrayBufferWriter<byte>(4096);

            using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
            {
                JsonSerializer.Serialize(writer, obj, DefaultOptions);
                writer.Flush();                    // обязательно!
            }

            // Преобразуем UTF-8 байты в строку
            return Encoding.UTF8.GetString(bufferWriter.WrittenSpan);
        }

        /// <summary>Сериализация с отступами (для отладки)</summary>
        public static string ToJsonFormatted<T>(this T? obj)
        {
            var options = new JsonSerializerOptions(DefaultOptions) { WriteIndented = true };
            return JsonSerializer.Serialize(obj, options);
        }
    }

    // ===================================================================
    // ======================= КАСТОМНЫЕ КОНВЕРТЕРЫ =======================
    // ===================================================================

    /// <summary>
    /// Безопасный конвертер enum'ов:
    /// - При сериализации: если значение не определено в enum → пишет null
    /// - При десериализации: неизвестное значение → default(T)
    /// </summary>
    public class SafeEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var str = reader.GetString();
                    if (Enum.TryParse(str, true, out T result))
                        return result;
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    var num = reader.GetInt32();
                    if (Enum.IsDefined(typeof(T), num))
                        return (T)Enum.ToObject(typeof(T), num);
                }
            }
            catch { }

            // Любая ошибка или неизвестное значение → default
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                writer.WriteNullValue();
                return;
            }

            // Пишем как строку (как делал StringEnumConverter)
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Decimal → string без лишних нулей в конце (аналог "G29")
    /// </summary>
    public class DecimalG29ToStringConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return decimal.TryParse(str, out var d) ? d : 0m;
            }

            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDecimal();

            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // G29 — самый компактный формат без trailing zeros
            writer.WriteStringValue(value.ToString("G29"));
        }
    }

    /// <summary>
    /// DateTime → строка в формате HHmmss и обратно
    /// </summary>
    public class HHMMSSDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str) || str.Length < 6)
                return default;

            try
            {
                int h = int.Parse(str.Substring(0, 2));
                int m = int.Parse(str.Substring(2, 2));
                int s = int.Parse(str.Substring(4, 2));

                var now = DateTime.Now;
                return new DateTime(now.Year, now.Month, now.Day, h, m, s);
            }
            catch
            {
                return default;
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("HHmmss"));
        }
    }

    /// <summary>
    /// Вспомогательные методы для GZip (оставлены без изменений)
    /// </summary>
    internal static class ZipExtensions
    {
        internal static byte[] GZip(this byte[] bytes)
        {
            using var inStream = new MemoryStream(bytes);
            using var outStream = new MemoryStream();
            using (var compress = new GZipStream(outStream, CompressionMode.Compress))
            {
                inStream.CopyTo(compress);
            }
            return outStream.ToArray();
        }

        internal static byte[] UnGZip(this byte[] bytes)
        {
            using var inStream = new MemoryStream(bytes);
            using var outStream = new MemoryStream();
            using (var decompress = new GZipStream(inStream, CompressionMode.Decompress))
            {
                decompress.CopyTo(outStream);
            }
            return outStream.ToArray();
        }

        internal static byte[] ToZipBytes(this string value) =>
            Encoding.UTF8.GetBytes(value).GZip();

        internal static string FromZipBytes(this byte[] bytes) =>
            Encoding.UTF8.GetString(bytes.UnGZip());
    }
}