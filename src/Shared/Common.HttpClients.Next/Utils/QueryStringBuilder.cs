#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Common.HttpClients.Utils
{
    /// <summary>
    /// 查询字符串构建工具，支持匿名对象、字典等类型转换为 URL 查询参数
    /// </summary>
    internal static class QueryStringBuilder
    {
        /// <summary>
        /// 将查询参数附加到 URL
        /// </summary>
        /// <param name="url">原始 URL</param>
        /// <param name="queryParameters">查询参数对象（匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <returns>附加了查询参数的 URL</returns>
        public static string AppendQuery(string url, object? queryParameters)
        {
            if (queryParameters == null)
            {
                return url;
            }

            var pairs = ExtractKeyValuePairs(queryParameters);
            if (pairs.Count == 0)
            {
                return url;
            }

            var queryString = BuildQueryString(pairs);
            if (string.IsNullOrEmpty(queryString))
            {
                return url;
            }

            var separator = url.Contains('?') ? "&" : "?";
            return $"{url}{separator}{queryString}";
        }

        private static List<KeyValuePair<string, string?>> ExtractKeyValuePairs(object parameters)
        {
            var pairs = new List<KeyValuePair<string, string?>>();

            switch (parameters)
            {
                case IDictionary<string, string?> dict:
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value != null)
                        {
                            pairs.Add(new KeyValuePair<string, string?>(kvp.Key, kvp.Value));
                        }
                    }
                    return pairs;

                case IDictionary<string, object?> dictObj:
                    foreach (var kvp in dictObj)
                    {
                        AddValue(pairs, kvp.Key, kvp.Value);
                    }
                    return pairs;

                case NameValueCollection nvc:
                    foreach (var key in nvc.AllKeys)
                    {
                        if (key != null)
                        {
                            AddValue(pairs, key, nvc[key]);
                        }
                    }
                    return pairs;
            }

            // 匿名对象或其他类型：通过反射读取属性
            var type = parameters.GetType();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead)
                {
                    continue;
                }

                var value = property.GetValue(parameters);
                AddValue(pairs, property.Name, value);
            }

            return pairs;
        }

        private static void AddValue(List<KeyValuePair<string, string?>> pairs, string key, object? value)
        {
            if (value == null)
            {
                return;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        pairs.Add(new KeyValuePair<string, string?>(key, ConvertToString(item)));
                    }
                }
            }
            else
            {
                pairs.Add(new KeyValuePair<string, string?>(key, ConvertToString(value)));
            }
        }

        private static string? ConvertToString(object value)
        {
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"),
                bool b => b ? "true" : "false",
                _ => value.ToString()
            };
        }

        private static string BuildQueryString(List<KeyValuePair<string, string?>> pairs)
        {
            var sb = new StringBuilder();
            foreach (var kvp in pairs)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.Append(WebUtility.UrlEncode(kvp.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(kvp.Value));
            }

            return sb.ToString();
        }
    }
}
