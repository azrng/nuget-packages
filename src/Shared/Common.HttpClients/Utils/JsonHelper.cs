using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Common.HttpClients.Utils
{
    internal static class JsonHelper
    {
        /// <summary>
        /// 对象转json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(object obj)
        {
            var settings = new JsonSerializerSettings
                           {
                               Formatting = Formatting.None, ContractResolver = new CamelCasePropertyNamesContractResolver()
                           };

            settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        /// json字符串转对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ToObject<T>(string json)
        {
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }
    }
}