namespace Common.HttpClients
{
    /// <summary>
    /// HttpClient配置
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// 失败是否抛出异常
        /// </summary>
        public bool FailThrowException { get; set; }

        /// <summary>
        /// 启用审计日志
        /// </summary>
        public bool AuditLog { get; set; } = true;

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 100;

        /// <summary>
        /// 最大输出响应长度
        /// </summary>
        public int MaxOutputResponseLength { get; set; }

        /// <summary>
        /// 是否忽略不安全证书
        /// </summary>
        public bool IgnoreUntrustedCertificate { get; set; } = false;

        /// <summary>
        /// 401未授权错误时是否重试
        /// </summary>
        public bool RetryOnUnauthorized { get; set; } = false;
    }
}