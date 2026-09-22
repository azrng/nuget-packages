using System.Collections.Generic;

namespace Common.HttpClients
{
    /// <summary>
    /// HTTP日志脱敏器。
    /// </summary>
    public interface IHttpLogRedactor
    {
        /// <summary>
        /// 对日志内容进行脱敏。
        /// </summary>
        /// <param name="content">原始日志内容。</param>
        /// <returns>脱敏后的日志内容。</returns>
        string RedactContent(string content);

        /// <summary>
        /// 对日志请求头或响应头进行脱敏。
        /// </summary>
        /// <param name="headers">原始请求头或响应头。</param>
        /// <returns>脱敏后的请求头或响应头。</returns>
        IDictionary<string, string> RedactHeaders(IDictionary<string, string>? headers);
    }
}
