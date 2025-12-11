using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.AspNetCore.Core.JsonConverters
{
    /// <summary>
    /// long 转字符串
    /// </summary>
    public class LongToStringConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 从字符串反序列化为 long
            if (reader.TokenType == JsonTokenType.String)
            {
                if (long.TryParse(reader.GetString(), out var value))
                    return value;
            }

            // 如果是数字，直接读取
            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            // 序列化时转为字符串
            writer.WriteStringValue(value.ToString());
        }
    }
}