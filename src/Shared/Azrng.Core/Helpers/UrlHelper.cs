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
        /// 提取字符串里面的URL地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ExtractUrl(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            var matches = Regex.Matches(str, "[a-zA-Z]+://[^\\s]*", RegexOptions.Multiline);
            foreach (Match match in matches.Cast<Match>())
            {
                if (!match.Success)
                    continue;

                if (Uri.TryCreate(match.Value, UriKind.Absolute, out var uri))
                {
                    return uri.GetLeftPart(UriPartial.Authority);
                }
            }

            return string.Empty;
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
        public static string AddQueryString(Uri uri, IEnumerable<KeyValuePair<string, string>> queryString)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            return AddQueryString(uri.ToString().TrimEnd('/'), queryString);
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
                return uri;

            var fragmentIndex = uri.IndexOf('#');
            var fragment = fragmentIndex >= 0 ? uri.Substring(fragmentIndex) : string.Empty;
            var body = fragmentIndex >= 0 ? uri.Substring(0, fragmentIndex) : uri;

            // 过滤掉无效键值对，避免生成错误的查询参数
            var encodedPairs = queryString.Where(pair => pair.Value != null && !pair.Key.IsNullOrWhiteSpace())
                                          .Select(pair => $"{pair.Key.UrlEncode()}={pair.Value.UrlEncode()}")
                                          .ToList();

            if (encodedPairs.Count == 0)
                return uri;

            var stringBuilder = new StringBuilder(body);
            var hasQuery = body.Contains('?');
            if (!hasQuery)
            {
                stringBuilder.Append('?');
            }
            else if (!body.EndsWith('?') && !body.EndsWith('&'))
            {
                // 如果原始 URL 已经拥有查询部分但未以连接符结尾，需要主动补上 &
                stringBuilder.Append('&');
            }

            stringBuilder.Append(string.Join("&", encodedPairs));
            stringBuilder.Append(fragment);
            return stringBuilder.ToString();
        }
    }
}