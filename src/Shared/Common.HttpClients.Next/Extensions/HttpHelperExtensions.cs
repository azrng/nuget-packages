using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Common.HttpClients
{
    /// <summary>
    /// <see cref="IHttpHelper"/> 与 <see cref="IHttpResult{T}"/> 的扩展方法
    /// </summary>
    public static class HttpHelperExtensions
    {
        /// <summary>
        /// 创建 Bearer Token 请求头字典
        /// </summary>
        /// <param name="token">Bearer Token（自动添加 "Bearer " 前缀）</param>
        /// <returns>包含 Authorization 头的字典</returns>
        public static IDictionary<string, string> CreateBearerHeaders(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var value = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token : "Bearer " + token;
            return new Dictionary<string, string> { ["Authorization"] = value };
        }

        /// <summary>
        /// 校验请求结果，失败时抛出 <see cref="HttpRequestException"/>，成功时返回自身以便链式取值
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="result">请求结果</param>
        /// <returns>成功的结果自身</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> 为 null</exception>
        /// <exception cref="HttpRequestException">请求失败（<see cref="IHttpResult{T}.IsSuccess"/> 为 false）</exception>
        public static IHttpResult<T> EnsureSuccess<T>(this IHttpResult<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!result.IsSuccess)
            {
                throw new HttpRequestException(
                    $"Request failed with status code {(int)result.StatusCode} ({result.StatusCode}).",
                    null,
                    result.StatusCode);
            }

            return result;
        }
    }
}
