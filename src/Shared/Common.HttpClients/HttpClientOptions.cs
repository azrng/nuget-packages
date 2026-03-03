using System.Collections.Generic;

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
        /// 是否启用日志脱敏
        /// </summary>
        public bool EnableLogRedaction { get; set; } = true;

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

        /// <summary>
        /// 并发限制数量
        /// </summary>
        public int ConcurrencyLimit { get; set; } = 100;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// 重试基础延迟时间（秒）
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 1;

        /// <summary>
        /// 额外需要脱敏的请求头名称（不区分大小写）
        /// </summary>
        public ICollection<string> AdditionalSensitiveHeaders { get; set; } = new List<string>();

        /// <summary>
        /// 额外需要脱敏的字段名（用于json和key=value文本）
        /// </summary>
        public ICollection<string> AdditionalSensitiveFields { get; set; } = new List<string>();
    }
}
