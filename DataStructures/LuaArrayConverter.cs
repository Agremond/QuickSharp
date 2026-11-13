using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuikSharp.DataStructures
{
    /// <summary>
    /// Массив из Lua: пустая таблица кодируется в JSON как {} (объект), а не [] — например,
    /// пустая сторона стакана; а getQuoteLevel2 для пустой стороны отдаёт и пустую строку "". Без этого
    /// весь стакан не разбирался, и терялась и непустая сторона.
    /// </summary>
    public sealed class LuaArrayConverter<T> : JsonConverter<T[]>
    {
        public override T[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.StartObject:
                    reader.Skip();
                    return Array.Empty<T>();
                case JsonTokenType.String:
                    // getQuoteLevel2 без котировок на стороне отдаёт "" вместо таблицы
                    return Array.Empty<T>();
                default:
                    return JsonSerializer.Deserialize<T[]>(ref reader, options);
            }
        }

        public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, options);
    }
}
