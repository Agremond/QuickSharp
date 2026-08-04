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
            {
                if (reader.TryGetInt32(out var intValue))
                    return intValue;

                // QUIK иногда отдаёт целочисленные поля как float (например "qty":1.0) —
                // TryGetInt32 отклоняет любой числовой токен с дробной частью, даже нулевой.
                // В этом случае читаем как double и округляем.
                return (int)Math.Round(reader.GetDouble());
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to int");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Номера заявок в QUIK — 18-19-значные целые (например 1892951937447283712), значительно
    /// превышающие точное представление double (~15-17 значащих цифр). Если такое поле читать
    /// как double (как раньше было с Order.OrderNum), последние цифры округляются и итоговое
    /// значение перестаёт совпадать с реальным номером заявки — из-за этого, например, попытка
    /// отменить заявку по ORDER_KEY, построенному из округлённого double, уходит на другой
    /// (или несуществующий) номер и молча отклоняется биржей. Поэтому такие поля объявлены как
    /// long, а этот конвертер регистрируется глобально в QuikJson.Options.
    /// </summary>
    public class StringToLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return long.TryParse(str, out var value) ? value : 0;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out var longValue))
                    return longValue;

                // Дробно-отформatированное число (как "qty":1.0) — восстанавливаем через
                // double. Для очень больших номеров заявок это уже не гарантирует точность
                // (double теряет точность после ~15-17 значащих цифр), но это единственный
                // доступный fallback для нецелого JSON-токена.
                return (long)Math.Round(reader.GetDouble());
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to long");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
