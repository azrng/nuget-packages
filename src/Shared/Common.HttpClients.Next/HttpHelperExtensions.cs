using System;
using System.Collections.Generic;

namespace Common.HttpClients
{
    /// <summary>
    /// <see cref="IHttpHelper"/> 的扩展方法
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
            var t = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token : "Bearer " + token;
            return new Dictionary<string, string> { ["Authorization"] = t };
        }
    }
}
