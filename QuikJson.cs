using System.Text.Json;
using System.Text.Json.Serialization;

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
            // Если нужно, можно добавить кастомный конвертер сюда
            // Options.Converters.Add(new EmptyArrayConverter());
        }
    }
}