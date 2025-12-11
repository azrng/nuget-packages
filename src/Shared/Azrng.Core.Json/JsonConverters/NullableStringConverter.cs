using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json.JsonConverters
{
    /// <summary>
    /// null 字符串转换器
    /// </summary>
    public class NullableStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            else
            {
                return value;
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }

            writer.WriteStringValue(value);
        }
    }
}