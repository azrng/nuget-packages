namespace Azrng.Core.Extension
{
    /// <summary>
    /// json扩展
    /// </summary>
    public static class JsonExtensions
    {
        // /// <summary>
        // /// 对象转json字符串
        // /// </summary>
        // /// <param name="obj"></param>
        // /// <returns></returns>
        // public static string ToJson(this object obj)
        // {
        //     var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
        //     return JsonConvert.SerializeObject(obj, timeConverter);
        // }
        //
        // /// <summary>
        // /// 对象转json字符串
        // /// </summary>
        // /// <param name="obj"></param>
        // /// <param name="datetimeFormats">设置时间格式</param>
        // /// <returns></returns>
        // public static string ToJson(this object obj, string datetimeFormats)
        // {
        //     var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeFormats };
        //     return JsonConvert.SerializeObject(obj, timeConverter);
        // }
        //
        // /// <summary>
        // /// json字符串转对象
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="json"></param>
        // /// <returns></returns>
        // public static T ToObject<T>(this string json)
        // {
        //     return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        // }
        //
        // /// <summary>
        // /// json字符串转list
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="json"></param>
        // /// <returns></returns>
        // public static List<T> ToList<T>(this string json)
        // {
        //     return json == null ? null : JsonConvert.DeserializeObject<List<T>>(json);
        // }
        //
        // /// <summary>
        // /// json字符串转DataTable
        // /// </summary>
        // /// <param name="json"></param>
        // /// <returns></returns>
        // public static DataTable ToTable(this string json)
        // {
        //     return json == null ? null : JsonConvert.DeserializeObject<DataTable>(json);
        // }
        //
        // /// <summary>
        // /// json字符串转JObject
        // /// </summary>
        // /// <param name="json"></param>
        // /// <returns></returns>
        // public static JObject ToJObject(this string json)
        // {
        //     return json == null ? JObject.Parse("{}") : JObject.Parse(json.Replace("&nbsp;", ""));
        // }

        /// <summary>
        /// 验证json字符串是否是JArray
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        public static bool IsJArrayString(this string jsonStr)
        {
            return !string.IsNullOrWhiteSpace(jsonStr) && jsonStr.Replace(" ", "").StartsWith("[");
        }

        //
        // /// <summary>
        // /// 是否是json字符串
        // /// </summary>
        // /// <param name="jsonStr"></param>
        // /// <returns></returns>
        // public static bool IsJsonString(this string jsonStr)
        // {
        //     try
        //     {
        //         JsonConvert.DeserializeObject<object>(jsonStr);
        //         return true;
        //     }
        //     catch
        //     {
        //         return false;
        //     }
        // }
    }
}