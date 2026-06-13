using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.HttpClients.Utils
{
    /// <summary>
    /// JSON序列化和反序列化辅助类（基于 System.Text.Json）
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// 序列化配置：CamelCase + UnsafeRelaxedJsonEscaping + IgnoreCycles
        /// </summary>
        private static readonly JsonSerializerOptions SerializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = false
        };

        /// <summary>
        /// 反序列化配置：在序列化配置基础上增加枚举字符串转换器
        /// </summary>
        private static readonly JsonSerializerOptions DeserializeOptions;

        static JsonHelper()
        {
            DeserializeOptions = new JsonSerializerOptions(SerializeOptions);
            DeserializeOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// 将对象序列化为JSON字符串（使用驼峰命名）
        /// </summary>
        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, SerializeOptions);
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        public static T? ToObject<T>(string? json)
        {
            return json == null ? default : JsonSerializer.Deserialize<T>(json, DeserializeOptions);
        }
    }
}
