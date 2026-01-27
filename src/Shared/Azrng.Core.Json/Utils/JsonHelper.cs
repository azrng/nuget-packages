using Azrng.Core.Extension;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json.Utils
{
    public static class JsonHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public static string Serialize(object obj, JsonSerializerOptions? serializerSettings = null)
        {
            if (serializerSettings is not null)
                return JsonSerializer.Serialize(obj, serializerSettings);

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
                                                 {
                                                     // 属性名不区分大小写
                                                     PropertyNameCaseInsensitive = true,
                                                     PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                                                     DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                                                     Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关闭默认转义
                                                     ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
                                                     ReadCommentHandling = JsonCommentHandling.Skip, //跳过注释
                                                     AllowTrailingCommas = true, // 允许尾随逗号
                                                 });
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public static T? Deserialize<T>(string? json, JsonSerializerOptions? serializerSettings = null)
        {
            if (json.IsNullOrWhiteSpace())
                return default;

            if (serializerSettings is not null)
                return JsonSerializer.Deserialize<T>(json, serializerSettings);

            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                                                       {
                                                           // 属性名不区分大小写
                                                           PropertyNameCaseInsensitive = true,
                                                           PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                                                           DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                                                           Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关闭默认转义
                                                           ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
                                                           ReadCommentHandling = JsonCommentHandling.Skip, //跳过注释
                                                           AllowTrailingCommas = true, // 允许尾随逗号
                                                       });
        }

        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <typeparam name="T">原始类型</typeparam>
        /// <param name="obj">原对象</param>
        /// <returns></returns>
        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification =
                "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
        [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
        public static T? Clone<T>(T? obj) where T : class
        {
            if (obj is null)
                return null;

            var jsonString = JsonSerializer.SerializeToUtf8Bytes(obj);
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// 校验字符串是否是json字符串（通过try catch实现）
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool IsJsonString(this string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                return false;
            try
            {
                JsonSerializer.Deserialize<object>(jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}