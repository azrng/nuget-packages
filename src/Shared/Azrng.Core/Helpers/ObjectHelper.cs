using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Azrng.Core.Helpers
{
    public class ObjectHelper
    {
        /// <summary>
        /// 获取对象中所有字符串类型属性的名称和值
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">要读取的对象</param>
        /// <param name="includeEmpty">是否包含空字符串</param>
        /// <returns>属性名称和值的字典</returns>
        public static Dictionary<string, string> GetStringPropertiesWithValues<T>(T obj, bool includeEmpty = false)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var result = new Dictionary<string, string>();

            // 获取所有公共实例属性
            var properties = typeof(T)
                             .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.PropertyType == typeof(string)); // 只选择字符串类型

            foreach (var property in properties)
            {
                // 检查属性是否有getter
                if (!property.CanRead)
                    continue;

                try
                {
                    // 根据includeEmpty参数决定是否包含空值
                    if (property.GetValue(obj) is string value && (includeEmpty || !string.IsNullOrEmpty(value)))
                    {
                        result[property.Name] = value;
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续处理其他属性
                    Console.WriteLine($"读取属性 {property.Name} 时出错: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// 将对象转换为目标类型的值
        /// </summary>
        public static object ConvertToTargetType(object value, Type targetType)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            // 处理可空类型
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // 获取可空类型的实际类型
                targetType = Nullable.GetUnderlyingType(targetType);

                // 如果值为null，则对于可空类型来说是有效的
                if (value is string str && string.IsNullOrEmpty(str))
                    return null;
            }

            var valueType = value.GetType();

            // 直接匹配类型，避免多次GetType调用
            if (valueType == targetType || (Nullable.GetUnderlyingType(targetType!) ?? targetType) == valueType)
                return value;

            var typeCode = Type.GetTypeCode(targetType);
            var isEnum = targetType.BaseType == typeof(Enum);
            return typeCode switch
            {
                TypeCode.Int32 => isEnum ? HandleSpecialTypes(value, targetType) : Convert.ToInt32(value),
                TypeCode.Int64 => Convert.ToInt64(value),
                TypeCode.Decimal => Convert.ToDecimal(value),
                TypeCode.Double => Convert.ToDouble(value),
                TypeCode.Single => Convert.ToSingle(value),
                TypeCode.String => value.ToString(),
                TypeCode.DateTime => Convert.ToDateTime(value),
                TypeCode.Boolean => Convert.ToBoolean(value),
                TypeCode.Byte => Convert.ToByte(value),
                TypeCode.Char => Convert.ToChar(value),
                TypeCode.Int16 => Convert.ToInt16(value),
                TypeCode.SByte => Convert.ToSByte(value),
                TypeCode.UInt16 => Convert.ToUInt16(value),
                TypeCode.UInt32 => Convert.ToUInt32(value),
                TypeCode.UInt64 => Convert.ToUInt64(value),
                _ => HandleSpecialTypes(value, targetType)
            };
        }

        /// <summary>
        /// 处理特殊类型的转换
        /// </summary>
        private static object HandleSpecialTypes(object value, Type targetType)
        {
            if (targetType.IsEnum)
            {
                return value switch
                {
                    string stringValue => Enum.Parse(targetType, stringValue),
                    _ => Enum.ToObject(targetType, Convert.ToInt32(value))
                };
            }

            // 默认使用 Convert.ChangeType 转换
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 如果转换失败，返回原始值
                return value;
            }
        }
    }
}