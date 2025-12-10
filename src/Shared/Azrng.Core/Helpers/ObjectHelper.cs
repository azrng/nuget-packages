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
    }
}