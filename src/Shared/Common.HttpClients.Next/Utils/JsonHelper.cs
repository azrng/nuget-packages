using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.HttpClients.Utils
{
    /// <summary>
    /// JSON序列化和反序列化辅助类（基于 System.Text.Json），按命名策略缓存 JsonSerializerOptions
    /// </summary>
    internal static class JsonHelper
    {
        private static readonly ConcurrentDictionary<JsonNamingPolicyType, (JsonSerializerOptions Serialize, JsonSerializerOptions Deserialize)> OptionsCache = new();

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        public static string ToJson(object obj, JsonNamingPolicyType namingPolicy = JsonNamingPolicyType.CamelCase)
        {
            var (serialize, _) = GetOptions(namingPolicy);
            return JsonSerializer.Serialize(obj, serialize);
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        public static T? ToObject<T>(string? json, JsonNamingPolicyType namingPolicy = JsonNamingPolicyType.CamelCase)
        {
            if (json == null)
            {
                return default;
            }

            var (_, deserialize) = GetOptions(namingPolicy);
            return JsonSerializer.Deserialize<T>(json, deserialize);
        }

        private static (JsonSerializerOptions Serialize, JsonSerializerOptions Deserialize) GetOptions(JsonNamingPolicyType namingPolicy)
        {
            return OptionsCache.GetOrAdd(namingPolicy, BuildOptions);
        }

        private static (JsonSerializerOptions Serialize, JsonSerializerOptions Deserialize) BuildOptions(JsonNamingPolicyType namingPolicy)
        {
            JsonNamingPolicy? policy = namingPolicy switch
            {
                JsonNamingPolicyType.CamelCase => JsonNamingPolicy.CamelCase,
                JsonNamingPolicyType.PascalCase => PascalCaseNamingPolicy.Instance,
                JsonNamingPolicyType.SnakeCaseLower => SnakeCaseLowerNamingPolicy.Instance,
                _ => null
            };

            var serialize = new JsonSerializerOptions
            {
                PropertyNamingPolicy = policy,
                DictionaryKeyPolicy = policy,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = false
            };

            var deserialize = new JsonSerializerOptions(serialize);
            deserialize.Converters.Add(new JsonStringEnumConverter());

            return (serialize, deserialize);
        }
    }

    /// <summary>
    /// 帕斯卡命名策略：首字母大写（net6/7 无内置，统一自定义实现）
    /// </summary>
    internal sealed class PascalCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly PascalCaseNamingPolicy Instance = new();

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
    }

    /// <summary>
    /// 小写下划线命名策略：大小写边界插入下划线并转小写（net6/7 无内置，统一自定义实现）
    /// </summary>
    internal sealed class SnakeCaseLowerNamingPolicy : JsonNamingPolicy
    {
        public static readonly SnakeCaseLowerNamingPolicy Instance = new();

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var sb = new StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c))
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }
    }
}
