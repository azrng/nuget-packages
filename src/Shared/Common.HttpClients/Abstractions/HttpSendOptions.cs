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
    }
}
