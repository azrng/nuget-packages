using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Azrng.Core.Json.Utils
{
    public static class JsonHelper
    {
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