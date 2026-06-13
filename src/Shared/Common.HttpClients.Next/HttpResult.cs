using System.Net;

namespace Common.HttpClients
{
    /// <summary>
    /// <see cref="IHttpResult{T}"/> 的内部实现
    /// </summary>
    /// <typeparam name="T">响应数据类型</typeparam>
    internal sealed class HttpResult<T> : IHttpResult<T>
    {
        /// <inheritdoc />
        public bool IsSuccess { get; }

        /// <inheritdoc />
        public T? Data { get; }

        /// <inheritdoc />
        public string? ErrorMessage { get; }

        /// <inheritdoc />
        public HttpStatusCode StatusCode { get; }

        /// <inheritdoc />
        public string? RawBody { get; }

        /// <inheritdoc />
        public bool IsFallbackResponse { get; }

        private HttpResult(bool isSuccess, T? data, string? errorMessage,
                           HttpStatusCode statusCode, string? rawBody, bool isFallbackResponse)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            RawBody = rawBody;
            IsFallbackResponse = isFallbackResponse;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="data">响应数据</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="rawBody">原始响应体</param>
        /// <returns>成功的HttpResult</returns>
        public static HttpResult<T> Success(T? data, HttpStatusCode statusCode, string? rawBody)
        {
            return new HttpResult<T>(true, data, null, statusCode, rawBody, false);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="rawBody">原始响应体</param>
        /// <param name="isFallbackResponse">是否为降级响应</param>
        /// <returns>失败的HttpResult</returns>
        public static HttpResult<T> Fail(string? errorMessage, HttpStatusCode statusCode,
                                         string? rawBody, bool isFallbackResponse)
        {
            return new HttpResult<T>(false, default, errorMessage, statusCode, rawBody, isFallbackResponse);
        }
    }
}
