using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.Core.Json.JsonConverters
{
    /// <summary>
    /// 枚举转字符串
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class EnumStringConverter<TEnum> : JsonConverter<TEnum>
    {
        private readonly bool _isNullable;

        public EnumStringConverter(bool isNullType)
        {
            _isNullable = isNullType;
        }

        /// <summary>
        /// 判断当前类型是否可以使用该转换器转换
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType) => EnumStringConverterFactory.IsEnum(objectType);

        // 从 json 中读取数据
        // JSON => 值
        // typeToConvert: 模型类属性/字段的类型
        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 读取 json
            var value = reader.GetString();
            if (value == null)
            {
                if (_isNullable) return default;
                throw new ArgumentNullException(nameof(value));
            }

            // 是否为可空类型
            var sourceType = EnumStringConverterFactory.GetSourceType(typeof(TEnum));
            if (Enum.TryParse(sourceType, value, out var result))
            {
                return (TEnum)result!;
            }

            throw new InvalidOperationException($"{value} 值不在枚举 {typeof(TEnum).Name} 范围中");
        }

        /// <summary>
        /// 值 => JSON
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
        {
            if (value == null) writer.WriteNullValue();
            else writer.WriteStringValue(Enum.GetName(value.GetType(), value));
        }
    }

    public class EnumStringConverterFactory : JsonConverterFactory
    {
        // 获取需要转换的类型
        public static bool IsEnum(Type objectType)
        {
            if (objectType.IsEnum) return true;

            var sourceType = Nullable.GetUnderlyingType(objectType);
            return sourceType is not null && sourceType.IsEnum;
        }

        // 如果类型是可空类型，则获取原类型
        public static Type? GetSourceType(Type typeToConvert)
        {
            if (typeToConvert.IsEnum) return typeToConvert;
            return Nullable.GetUnderlyingType(typeToConvert);
        }

        // 判断该类型是否属于枚举
        public override bool CanConvert(Type typeToConvert) => IsEnum(typeToConvert);

        // 为该字段创建一个对应的类型转换器
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var sourceType = GetSourceType(typeToConvert);
            var converter = typeof(EnumStringConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter?)Activator.CreateInstance(converter, new object[]
                                                                      {
                                                                          sourceType != typeToConvert
                                                                      });
        }
    }
}