using System;
using System.Collections.Generic;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// Dictionary扩展
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// 获取或者返回默认值
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var @default))
                return defaultValue;
            return @default;
        }

        /// <summary>
        /// 根据列名获取列值
        /// </summary>
        /// <param name="keyValues">字典型数据结构</param>
        /// <param name="key">列名</param>
        public static string GetColumnValueByName(this IDictionary<string, string> keyValues, string key)
        {
            if (keyValues == null || key == null || !keyValues.TryGetValue(key, out var columnInfo))
            {
                return null;
            }

            return columnInfo;
        }

        /// <summary>
        /// 根据列名获取列值
        /// </summary>
        /// <param name="keyValues">字典型数据结构</param>
        /// <param name="key">列名</param>
        public static string GetColumnValueByName(this IDictionary<string, object> keyValues, string key)
        {
            if (keyValues == null || key == null || !keyValues.TryGetValue(key, out var columnInfo))
            {
                return "";
            }

            return columnInfo switch
            {
                DateTime _ => columnInfo.ToString()?.ToDateTime()?.ToStandardString() ?? "",
                decimal _ => columnInfo.ToString()?.ToDecimal().ToStandardString() ?? "",
                _ => columnInfo?.ToString() ?? ""
            };
        }

        /// <summary>
        /// 根据列名获取列值（可为null的类型返回null值）
        /// </summary>
        /// <param name="keyValues">字典型数据结构</param>
        /// <param name="key">列名</param>
        public static T GetColumnValueByName<T>(this IDictionary<string, object> keyValues, string key)
        {
            if (keyValues == null || key == null || !keyValues.TryGetValue(key, out var columnInfo))
            {
                return default;
            }

            if (columnInfo is null)
                return default;

            if (typeof(T) == typeof(int?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(int));
            }
            else if (typeof(T) == typeof(decimal?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(decimal));
            }
            else if (typeof(T) == typeof(double?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(double));
            }
            else if (typeof(T) == typeof(Guid?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(Guid));
            }
            else if (typeof(T) == typeof(bool?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(bool));
            }
            else if (typeof(T) == typeof(DateTime?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(DateTime));
            }
            else if (typeof(T) == typeof(long?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(long));
            }
            else if (typeof(T) == typeof(byte?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(byte));
            }
            else if (typeof(T) == typeof(short?))
            {
                return (T)Convert.ChangeType(columnInfo, typeof(short));
            }
            else
            {
                return (T)Convert.ChangeType(columnInfo, typeof(T));
            }
        }

        /// <summary>
        /// 当键存在时更新键值，当键不存在则追加键值
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddOrAppend(this IDictionary<string, int> dict, string key, int value)
        {
            if (dict == null)
                throw new ArgumentNullException();

            if (!dict.TryAdd(key, value))
            {
                dict[key] += value;
            }
        }
    }
}