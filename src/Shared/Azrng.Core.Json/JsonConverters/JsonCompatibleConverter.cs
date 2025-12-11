using System.Text.Json.Serialization;

namespace Azrng.Core.Json.JsonConverters
{
    /// <summary>
    /// 提供一些类型的兼容性的json转换器
    /// </summary>
    public static class JsonCompatibleConverter
    {
        private static JsonStringEnumConverter? _stringEnumConverter;

        /// <summary>
        /// 获取Enum类型反序列化兼容的转换器
        /// </summary>
        public static JsonConverter EnumReader
        {
            get
            {
                _stringEnumConverter ??= new JsonStringEnumConverter();
                return _stringEnumConverter;
            }
        }

        /// <summary>
        /// 获取DateTime类型反序列化兼容的转换器
        /// </summary>
        public static JsonConverter DateTimeReader { get; } = new JsonDateTimeConverter("O");
    }
}