using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json.JsonConverters
{
    /// <summary>
    /// DateTimeOffset 和字符串转换器
    /// </summary>
    public class DateTimeOffsetToStringConverter : JsonConverter<DateTimeOffset>
    {
        private readonly string _formatString;

        public DateTimeOffsetToStringConverter(string inFormatString = "yyyy-MM-dd HH:mm:ss")
        {
            _formatString = inFormatString;
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert,
                                            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString() ?? throw new FormatException("当前字段格式错误");
                return DateTimeOffset.ParseExact(value, _formatString, null);
            }

            return reader.GetDateTimeOffset();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_formatString));
        }
    }
}