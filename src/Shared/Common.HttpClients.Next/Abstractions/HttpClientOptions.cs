using System.Collections.Generic;

namespace Common.HttpClients
{
    /// <summary>
    /// JSON 命名策略（请求体序列化与响应反序列化统一适用）
    /// </summary>
    public enum JsonNamingPolicyType
    {
        /// <summary>驼峰命名（默认，与历史行为兼容）</summary>
        CamelCase = 0,

        /// <summary>帕斯卡命名（首字母大写）</summary>
        PascalCase = 1,

        /// <summary>小写下划线命名（snake_case）</summary>
        SnakeCaseLower = 2,

        /// <summary>不转换，按属性名原样输出</summary>
        None = 3
    }

    /// <summary>
    /// HttpClient配置
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// 基础地址，请求 URL 为相对路径时自动拼接
        /// </summary>
        public string? BaseAddress { get; set; }

        /// <summary>
        /// 默认请求头，每个请求自动携带（per-request headers 优先覆盖）；支持同名多值
        /// </summary>
        public HttpHeaders? DefaultHeaders { get; set; }

        /// <summary>
        /// 自定义 User-Agent
        /// </summary>
        public string? UserAgent { get; set; }

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
        /// 请求体日志最大输出长度
        /// </summary>
        public int MaxRequestBodyLength { get; set; } = 4096;

        /// <summary>
        /// 最大输出响应长度
        /// </summary>
        public int MaxOutputResponseLength { get; set; } = 4096;

        /// <summary>
        /// 是否忽略不安全证书
        /// </summary>
        public bool IgnoreUntrustedCertificate { get; set; } = false;

        /// <summary>
        /// 401未授权错误时是否重试
        /// </summary>
        public bool RetryOnUnauthorized { get; set; } = false;

        /// <summary>
        /// JSON 命名策略（请求体序列化与响应反序列化统一）；默认 CamelCase，与 3.x 行为兼容
        /// </summary>
        public JsonNamingPolicyType JsonNamingPolicy { get; set; } = JsonNamingPolicyType.CamelCase;

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
