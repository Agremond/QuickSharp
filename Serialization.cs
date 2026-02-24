using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace QuikSharp
{
    /// <summary>
    /// Lightweight JSON helpers (SHM architecture ready)
    /// </summary>
    public static class JsonExtensions
    {
        private static readonly JsonSerializerSettings Settings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore
            };

        [ThreadStatic]
        private static StringBuilder _stringBuilder;

        public static T FromJson<T>(this string json) =>
            JsonConvert.DeserializeObject<T>(json, Settings);

        public static object FromJson(this string json, Type type) =>
            JsonConvert.DeserializeObject(json, type, Settings);

        public static string ToJson<T>(this T obj)
        {
            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder(4096);

            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.None, Settings);
            }
            finally
            {
                _stringBuilder.Clear();
            }
        }

        public static string ToJsonFormatted<T>(this T obj) =>
            JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
    }

    /// <summary>
    /// Limits enum serialization only to defined values
    /// </summary>
    public class SafeEnumConverter<T> : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!Enum.IsDefined(typeof(T), value))
                value = null;

            base.WriteJson(writer, value, serializer);
        }
    }

    /// <summary>
    /// Serialize Decimal to string without trailing zeros
    /// </summary>
    public class DecimalG29ToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(decimal);

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            return JToken.Load(reader).Value<decimal>();
        }

        public override void WriteJson(JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            var str = ((decimal)value).ToString("G29");
            writer.WriteValue(str);
        }
    }

    /// <summary>
    /// Convert DateTime to HHmmss
    /// </summary>
    public class HHMMSSDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(DateTime);

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var str = JToken.Load(reader).Value<string>();
            if (string.IsNullOrEmpty(str))
                return default;

            var now = DateTime.Now;
            return new DateTime(
                now.Year,
                now.Month,
                now.Day,
                int.Parse(str.Substring(0, 2)),
                int.Parse(str.Substring(2, 2)),
                int.Parse(str.Substring(4, 2)));
        }

        public override void WriteJson(JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString("HHmmss"));
        }
    }

    /// <summary>
    /// Shared ArrayPool for JSON.NET
    /// </summary>
    public sealed class JsonArrayPool : IArrayPool<char>
    {
        public static readonly JsonArrayPool Instance = new JsonArrayPool();

        public char[] Rent(int minimumLength) =>
            ArrayPool<char>.Shared.Rent(minimumLength);

        public void Return(char[] array) =>
            ArrayPool<char>.Shared.Return(array);
    }

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