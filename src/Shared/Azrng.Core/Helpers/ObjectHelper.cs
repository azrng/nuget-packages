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
            }

            if (targetType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else if (targetType == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            else if (targetType == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }
            else if (targetType == typeof(double))
            {
                return Convert.ToDouble(value);
            }
            else if (targetType == typeof(float))
            {
                return Convert.ToSingle(value);
            }
            else if (targetType == typeof(string))
            {
                return value?.ToString() ?? string.Empty;
            }
            else if (targetType == typeof(DateTime))
            {
                return Convert.ToDateTime(value);
            }
            else if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }
            else if (targetType!.IsEnum)
            {
                if (value is string stringValue)
                    return Enum.Parse(targetType, stringValue);
                else
                    return Enum.ToObject(targetType, Convert.ToInt32(value));
            }
            else
            {
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
}