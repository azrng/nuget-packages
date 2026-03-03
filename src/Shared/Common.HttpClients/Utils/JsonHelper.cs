using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Common.HttpClients.Utils
{
    /// <summary>
    /// JSON序列化和反序列化辅助类
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// 共享的JSON序列化配置，避免重复创建
        /// </summary>
        private static readonly JsonSerializerSettings Settings = CreateSettings();

        /// <summary>
        /// 将对象序列化为JSON字符串（使用驼峰命名）
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        /// <summary>
        /// 将JSON字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>反序列化的对象，输入为null时返回default(T)</returns>
        public static T ToObject<T>(string json)
        {
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 创建JSON序列化配置
        /// </summary>
        /// <returns>配置好的JsonSerializerSettings实例</returns>
        private static JsonSerializerSettings CreateSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            return settings;
        }
    }
}
