using Azrng.Core.Extension;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Azrng.Core.Json
{
    public class SysTextJsonSerializer : IJsonSerializer
    {
        private readonly DefaultJsonSerializerOptions _serializerConfig;

        public SysTextJsonSerializer(IOptions<DefaultJsonSerializerOptions> serializerOptions)
        {
            _serializerConfig = serializerOptions.Value;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public string ToJson<T>(T obj) where T : class
        {
            return JsonSerializer.Serialize(obj, _serializerConfig.JsonSerializeOptions);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public T? ToObject<T>(string json)
        {
            return json.IsNullOrWhiteSpace() ? default : JsonSerializer.Deserialize<T>(json, _serializerConfig.JsonDeserializeOptions);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public T? Clone<T>(T obj) where T : class
        {
            return ToObject<T>(ToJson(obj));
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public List<T>? ToList<T>(string json)
        {
            return json.IsNullOrWhiteSpace() ? null : JsonSerializer.Deserialize<List<T>>(json, _serializerConfig.JsonDeserializeOptions);
        }

        /// <summary>
        /// 通过 Key 获取 Value
        /// </summary>
        /// <returns></returns>
        public static string? GetValue(string json, string key)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty(key).ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 没有 Key 的 Json 转 List JToken
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonNode? JsonToArrayList(string json)
        {
            var jsonNode = JsonNode.Parse(json);

            return jsonNode;
        }
    }
}