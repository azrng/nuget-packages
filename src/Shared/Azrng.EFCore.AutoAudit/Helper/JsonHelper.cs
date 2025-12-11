using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.EFCore.AutoAudit.Helper
{
    internal static class JsonHelper
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, CreateJsonSerializeOptions());
        }

        public static T? ToObject<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, CreateJsonSerializeOptions());
        }

        /// <summary>
        /// 创建序列化JsonSerializerOptions
        /// </summary>
        private static JsonSerializerOptions CreateJsonSerializeOptions()
        {
            return new JsonSerializerOptions
                   {
                       // 属性名不区分大小写
                       PropertyNameCaseInsensitive = true,
                       PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                       DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关闭默认转义
                       ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
                   };
        }
    }
}