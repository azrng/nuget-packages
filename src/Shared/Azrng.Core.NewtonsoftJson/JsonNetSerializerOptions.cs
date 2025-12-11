using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Azrng.Core.NewtonsoftJson
{
    /// <summary>
    /// json.net选项
    /// </summary>
    public class JsonNetSerializerOptions
    {
        /// <summary>
        /// camelCaseResolver
        /// </summary>
        private static readonly IContractResolver _camelCaseResolver = new CamelCasePropertyNamesContractResolver();

        /// <summary>
        /// 序列化设置
        /// </summary>
        public JsonSerializerSettings JsonSerializeOptions { get; set; } = CreateDefaultJsonOptions();

        /// <summary>
        /// 反序列化设置
        /// </summary>
        public JsonSerializerSettings JsonDeserializeOptions { get; set; } = CreateDefaultJsonOptions();

        /// <summary>
        /// 创建默认JsonSerializerSettings
        /// </summary>
        private static JsonSerializerSettings CreateDefaultJsonOptions()
        {
            var settings = new JsonSerializerSettings { ContractResolver = _camelCaseResolver, };

            settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            return settings;
        }
    }
}