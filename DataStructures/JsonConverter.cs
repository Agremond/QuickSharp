using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace QUIKSharp.DataStructures
{

    using System.Text.Json;
    using System.Text.Json.Serialization;
    public class EmptyStringToArrayConverter<T> : JsonConverter<T[]?> where T : class
    {
        public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return Array.Empty<T>();   // пустая строка → пустой массив
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // обычный массив — десериализуем как обычно
                return JsonSerializer.Deserialize<T[]>(ref reader, options);
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // fallback — пытаемся десериализовать
            return JsonSerializer.Deserialize<T[]>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
    public class StringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return decimal.TryParse(str, System.Globalization.NumberStyles.Any,
                                       System.Globalization.CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0m;
            }
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDecimal();

            throw new JsonException($"Cannot convert {reader.TokenType} to decimal");
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    public class StringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return int.TryParse(str, out var value) ? value : 0;
            }
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            throw new JsonException($"Cannot convert {reader.TokenType} to int");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
