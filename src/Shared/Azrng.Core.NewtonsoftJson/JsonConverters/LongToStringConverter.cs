using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Azrng.Core.NewtonsoftJson.JsonConverters
{
    /// <summary>
    /// Newtonsoft long 转换string
    /// </summary>
    public class LongToStringConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jt = JToken.ReadFrom(reader);

            return jt.Value<long>();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long) == objectType;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));

            serializer.Serialize(writer, value?.ToString());
        }
    }
}