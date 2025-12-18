using System;
using System.Collections.Generic;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 类型扩展
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 获取自定义扩展
        /// </summary>
        /// <param name="sourceType">类型</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static T CustomAttributeCommon<T>(this Type sourceType, string fieldName) where T : Attribute
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return null;
            }

            var field = sourceType.GetField(fieldName);
            if (field == null)
            {
                return null;
            }

            var val = Attribute.GetCustomAttribute(field, typeof(T), inherit: false) as T;
            return val;
        }

        /// <summary>
        /// 获取枚举值和对应的特性
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<Enum, T> ToEnumAndAttributes<T>(this Type type) where T : Attribute
        {
            var values = Enum.GetValues(type);
            var dictionary = new Dictionary<Enum, T>();
            foreach (Enum item in values)
            {
                dictionary.Add(item, item.GetCustomerAttribute<T>());
            }

            return dictionary;
        }
    }
}