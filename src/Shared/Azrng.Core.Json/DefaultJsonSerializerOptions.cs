using Azrng.Core.Json.JsonConverters;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json
{
    /// <summary>
    /// 默认json序列化配置
    /// </summary>
    public class DefaultJsonSerializerOptions
    {
        /// <summary>
        /// 获取 json 序列化选项
        /// </summary>
        public JsonSerializerOptions JsonSerializeOptions { get; set; } = CreateJsonSerializeOptions();

        /// <summary>
        /// 获取 json 反序列化选项
        /// </summary>
        public JsonSerializerOptions JsonDeserializeOptions { get; set; } = CreateJsonDeserializeOptions();

        /// <summary>
        /// 创建反序列化JsonSerializerOptions
        /// </summary>
        /// <returns></returns>
        [UnconditionalSuppressMessage("Trimming", "IL3050",
            Justification = "JsonCompatibleConverter.EnumReader使用前已经判断RuntimeFeature.IsDynamicCodeSupported")]
        private static JsonSerializerOptions CreateJsonDeserializeOptions()
        {
            var options = CreateJsonSerializeOptions();
            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                options.Converters.Add(JsonCompatibleConverter.EnumReader);
            }

            options.Converters.Add(JsonCompatibleConverter.DateTimeReader);
            return options;
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
                       DictionaryKeyPolicy = JsonNamingPolicy.CamelCase, // 启用驼峰格式
                       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关闭默认转义
                       ReferenceHandler = ReferenceHandler.IgnoreCycles, // 忽略循环引用
                       ReadCommentHandling = JsonCommentHandling.Skip, //跳过注释
                       AllowTrailingCommas = true, // 允许尾随逗号
                   };
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// 插入指定的<see cref="System.Text.Json.Serialization.JsonSerializerContext"/>到所有序列化选项的TypeInfoResolverChain的最前位置
        /// </summary>
        /// <param name="context"></param>
        public void PrependJsonSerializerContext(System.Text.Json.Serialization.JsonSerializerContext context)
        {
            this.JsonSerializeOptions.TypeInfoResolverChain.Insert(0, context);
            this.JsonDeserializeOptions.TypeInfoResolverChain.Insert(0, context);
        }
#endif
    }
}