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
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key, TValue defaultValue)
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
        public static string? GetColumnValueByName(this IDictionary<string, string>? keyValues, string? key)
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
        public static string GetColumnValueByName(this IDictionary<string, object?>? keyValues, string? key)
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
        public static T? GetColumnValueByName<T>(this IDictionary<string, object?>? keyValues, string? key)
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
        /// dict创建或更新
        /// </summary>
        /// <typeparam name="TKey">字典键类型</typeparam>
        /// <typeparam name="TValue">字典值类型</typeparam>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <remarks>当键存在时更新键值，当键不存在则添加键值</remarks>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            dict[key] = value;
        }

        /// <summary>
        /// dict创建或新增
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void CreateOrInsert<TKey, TValue>(this IDictionary<TKey, List<TValue>> dict, TKey key,
                                                        TValue value)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            if (dict.TryGetValue(key, out var val))
            {
                val.Add(value);
            }
            else
            {
                dict.Add(key, [value]);
            }
        }

        /// <summary>
        /// 当键存在时更新键值，当键不存在则累加值
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddOrAppend<TKey>(this IDictionary<TKey, int> dict, TKey key, int value)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            if (!dict.TryAdd(key, value))
            {
                dict[key] += value;
            }
        }

        /// <summary>
        /// 当键存在时更新键值，当键不存在则累加值
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddOrAppend<TKey>(this IDictionary<TKey, long> dict, TKey key, long value)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            if (!dict.TryAdd(key, value))
            {
                dict[key] += value;
            }
        }
    }
}