using Azrng.Core.Extension;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Azrng.Core.NewtonsoftJson
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonNetSerializerOptions _serializerOptions;

        public NewtonsoftJsonSerializer(IOptions<JsonNetSerializerOptions> serializerOptions)
        {
            _serializerOptions = serializerOptions.Value;
        }

        public string ToJson<T>(T obj) where T : class
        {
            return JsonConvert.SerializeObject(obj, _serializerOptions.JsonSerializeOptions);
        }

        public T? ToObject<T>(string json)
        {
            return json.IsNullOrWhiteSpace() ? default : JsonConvert.DeserializeObject<T>(json, _serializerOptions.JsonDeserializeOptions);
        }

        public T? Clone<T>(T obj) where T : class
        {
            return ToObject<T>(ToJson(obj));
        }

        public List<T>? ToList<T>(string json)
        {
            return json.IsNullOrWhiteSpace()
                ? null
                : JsonConvert.DeserializeObject<List<T>>(json, _serializerOptions.JsonDeserializeOptions);
        }
    }
}