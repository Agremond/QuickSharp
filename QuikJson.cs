using System.Text.Json;
using System.Text.Json.Serialization;
using QUIKSharp.DataStructures;

namespace QuikSharp
{
    public static class QuikJson
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,

            // Основные настройки для QUIK (Lua)
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString |
                             JsonNumberHandling.AllowNamedFloatingPointLiterals,

            // КРИТИЧНО для избежания проблем с пустыми массивами из Lua
            WriteIndented = false,                    // обычно не нужен отступ в продакшене
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,

            // Дополнительно помогает в пограничных случаях
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        };

        // Опционально: можно добавить конвенцию, чтобы пустые коллекции всегда сериализовались как []
        static QuikJson()
        {
            // QUIK иногда отдаёт целочисленные поля как float (например "qty":1.0 в ответах
            // по заявкам) — стандартный int-конвертер System.Text.Json такие токены отклоняет.
            Options.Converters.Add(new StringToIntConverter());

            // Номера заявок (order_num и т.п.) — 18-19-значные целые; long сохраняет точность,
            // которую double не может (см. StringToLongConverter).
            Options.Converters.Add(new StringToLongConverter());
        }
    }
}