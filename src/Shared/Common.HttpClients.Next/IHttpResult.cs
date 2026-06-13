using System.Net;

namespace Common.HttpClients
{
    /// <summary>
    /// HTTP请求结果包装接口，携带成功/失败状态、数据和HTTP元信息
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    public interface IHttpResult<T>
    {
        /// <summary>
        /// 请求是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 响应数据（失败时为 default(T)）
        /// </summary>
        T? Data { get; }

        /// <summary>
        /// 错误信息（成功时为 null）
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// HTTP状态码
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// 原始响应体内容
        /// </summary>
        string? RawBody { get; }

        /// <summary>
        /// 是否为降级响应（请求失败后由 Polly Fallback 策略返回的合成响应）
        /// </summary>
        bool IsFallbackResponse { get; }
    }
}
