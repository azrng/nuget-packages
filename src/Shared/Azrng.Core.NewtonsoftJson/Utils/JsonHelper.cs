using Azrng.Core.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Azrng.Core.NewtonsoftJson.Utils
{
    /// <summary>
    /// Json 静态工具类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 对象转json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Obsolete]
        public static string ToJson(object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }

        /// <summary>
        /// json字符串转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        [Obsolete]
        public static T? ToObject<T>(string? json)
        {
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        public static string Serialize(object obj, JsonSerializerSettings? serializerSettings = null)
        {
            if (serializerSettings is not null)
                return JsonConvert.SerializeObject(obj, serializerSettings);

            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="serializerSettings"></param>
        /// <returns></returns>
        public static T? Deserialize<T>(string? json, JsonSerializerSettings? serializerSettings = null)
        {
            return json.IsNullOrWhiteSpace() ? default : JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        /// <summary>
        /// json字符串转list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static List<T>? ToList<T>(string? json)
        {
            return json == null ? null : JsonConvert.DeserializeObject<List<T>>(json);
        }

        /// <summary>
        /// 验证json字符串是否是JArray
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static bool IsJArrayString(string? jsonStr)
        {
            return !string.IsNullOrWhiteSpace(jsonStr) && jsonStr.Replace(" ", "").StartsWith("[");
        }

        /// <summary>
        /// 是否是json字符串
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static bool IsJsonString(string jsonStr)
        {
            try
            {
                JsonConvert.DeserializeObject<object>(jsonStr);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 深拷贝
        /// </summary>
        /// <typeparam name="T">原始类型</typeparam>
        /// <param name="obj">原对象</param>
        /// <returns></returns>
        public static T? Clone<T>(T? obj)
            where T : class
        {
            return obj == null
                ? null
                : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}