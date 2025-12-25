using Azrng.Core.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Azrng.Core.Helpers
{
    public class UrlHelper
    {
        /// <summary>
        /// 排序url参数值
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static (string url, string @params) SortUrlParameters(string url)
        {
            if (url.IsNullOrWhiteSpace())
                return (string.Empty, string.Empty);

            // 解析URL以获取查询字符串
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query).ToString();
            if (query.IsNullOrWhiteSpace())
                return (url, null)!;

            // 分割参数
            var parameters = HttpUtility.ParseQueryString(query!);
            var sortedParameters = parameters.AllKeys.OrderBy(k => k).Select(k => $"{k}={parameters[k]}");

            // 重新构建查询字符串
            var sortedQuery = string.Join("&", sortedParameters);

            // 构建并返回排序后的URL
            var param = uri.PathAndQuery.Replace(uri.Query, "?" + sortedQuery);
            return (uri.Scheme + "://" + uri.Host + param, sortedQuery);
        }

        /// <summary>
        /// 获取字符串里面的URL地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetUrl(string str)
        {
            return new Regex("[a-zA-z]+://[^\\s]*", RegexOptions.Multiline).Matches(str)
                                                                           .Where(item => item.Success)
                                                                           .Select(item => item.Value)
                                                                           .FirstOrDefault();
        }

        /// <summary>
        /// 将不带前缀的URL字符串拼接  http://
        /// </summary>
        /// <param name="urlStr"></param>
        /// <returns></returns>
        public static string ToUrlString(string urlStr)
        {
            if (string.IsNullOrWhiteSpace(urlStr))
                return string.Empty;

            urlStr = urlStr.Trim();
            if (!urlStr.StartsWith("http://") && !urlStr.StartsWith("https://"))
            {
                return "http://" + urlStr;
            }

            return urlStr;
        }

        /// <summary>
        /// 将querystring转字典
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictFromQueryString(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);

            var dict = new Dictionary<string, string>();
            foreach (var item in query)
            {
                var currKey = item?.ToString() ?? string.Empty;
                if (currKey.IsNullOrEmpty())
                    continue;

                dict.Add(item?.ToString() ?? string.Empty, query[currKey]);
            }

            return dict;
        }

        /// <summary>
        /// 将给定的查询键值附加到 URI 之中
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string>> queryString)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (queryString == null)
                throw new ArgumentNullException(nameof(queryString));
            var num = uri.IndexOf('#');
            var str1 = uri;
            var str2 = "";
            if (num != -1)
            {
                str2 = uri.Substring(num);
                str1 = uri.Substring(0, num);
            }

            var flag = str1.IndexOf('?') != -1;
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(str1);
            foreach (var keyValuePair in queryString)
            {
                if (keyValuePair.Value != null)
                {
                    stringBuilder.Append(flag ? '&' : '?');
                    stringBuilder.Append(keyValuePair.Key.UrlEncode());
                    stringBuilder.Append('=');
                    stringBuilder.Append(keyValuePair.Value.UrlEncode());
                    flag = true;
                }
            }

            stringBuilder.Append(str2);
            return stringBuilder.ToString();
        }
    }
}