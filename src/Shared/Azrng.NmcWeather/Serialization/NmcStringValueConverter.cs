using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.NmcWeather.Serialization;

internal sealed class NmcStringValueConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.True => bool.TrueString,
            JsonTokenType.False => bool.FalseString,
            _ => throw new JsonException($"无法将 JSON token {reader.TokenType} 转换为字符串。")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
