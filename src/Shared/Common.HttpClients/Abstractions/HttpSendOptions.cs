namespace Common.HttpClients
{
    /// <summary>
    /// 单次请求的可选配置，收拢查询参数与请求头，统一所有动词方法签名
    /// </summary>
    public sealed class HttpSendOptions
    {
        /// <summary>
        /// 查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection），自动拼接到 URL
        /// </summary>
        public object? Query { get; set; }

        /// <summary>
        /// 请求头（per-request，覆盖客户端默认头）；支持同名多值
        /// </summary>
        public HttpHeaders? Headers { get; set; }

        /// <summary>
        /// 本次请求是否启用重试；默认 null 跟随客户端全局 <c>MaxRetryAttempts</c> 配置。
        /// token 换发、委托授权等非幂等端点应显式设置为 <c>false</c>，避免超时/5xx 后重发放大请求
        /// </summary>
        public bool? EnableRetry { get; set; }
    }
}
