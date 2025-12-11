using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json.JsonConverters
{
    /// <summary>
    /// 时间转换器
    /// </summary>
    public class DateTimeToStringConverter : JsonConverter<DateTime>
    {
        private readonly string _formatString;

        public DateTimeToStringConverter()
        {
            _formatString = "yyyy-MM-dd HH:mm:ss";
        }

        public DateTimeToStringConverter(string inFormatString)
        {
            _formatString = inFormatString;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString() ?? throw new FormatException("当前字段格式错误");
                return DateTime.ParseExact(value, _formatString, null);
            }

            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formatString));
        }
    }
}