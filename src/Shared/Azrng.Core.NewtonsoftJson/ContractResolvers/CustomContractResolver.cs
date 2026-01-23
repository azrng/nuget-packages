using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Azrng.Core.NewtonsoftJson.ContractResolvers
{
    /// <summary>
    /// 自定义契约解析器
    /// </summary>
    public class CustomContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>将long类型转字符串</summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        protected override JsonConverter? ResolveContractConverter(Type objectType)
        {
            return objectType == typeof (long) ? new JsonConverterLong() : base.ResolveContractConverter(objectType);
        }
    }
}