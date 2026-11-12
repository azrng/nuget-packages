using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.AspNetCore.Core.JsonConverters
{
    /// <summary>
    /// long? 转字符串，JSON null 或空串反序列化为 null，其余行为与 LongToStringConverter 一致
    /// </summary>
    public class LongToNullableStringConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.String:
                    var text = reader.GetString();
                    if (string.IsNullOrEmpty(text))
                    {
                        // 空串按 null 处理，避免前端传 "" 时反序列化失败
                        return null;
                    }

                    if (long.TryParse(text, out var value))
                        return value;

                    throw new JsonException($"无法将 \"{text}\" 解析为 long?");
            }

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
