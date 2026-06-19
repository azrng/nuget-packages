namespace Azrng.NMaxCompute.Models;

/// <summary>
/// MaxCompute 直连配置
/// </summary>
public class MaxComputeConfig
{
    /// <summary>
    /// ODPS REST API 端点，如 <c>http://service.cn-hangzhou.maxcompute.aliyun.com/api</c>
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// AccessKey ID
    /// </summary>
    public string AccessId { get; set; } = string.Empty;

    /// <summary>
    /// AccessKey Secret
    /// </summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 项目名（必填）
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Schema 名（MC 2.0，可选）
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// region（如 <c>cn-hangzhou</c>），用于 V4 签名。为 null 时使用 V1 签名
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// STS 临时凭证 SecurityToken（可选）
    /// </summary>
    public string? SecurityToken { get; set; }

    /// <summary>
    /// 自定义 Tunnel 端点（可选，默认从 endpoint 推断）
    /// </summary>
    public string? TunnelEndpoint { get; set; }

    /// <summary>
    /// 最大返回行数（默认 10000）
    /// </summary>
    public int MaxRows { get; set; } = 10000;

    /// <summary>
    /// 是否优先使用 V4 签名（默认 true，仅在 Region 非空时生效）
    /// </summary>
    public bool UseV4Signature { get; set; } = true;

    /// <summary>
    /// 全局 SQL hints（注入到每个 SQLTask 的 settings）
    /// </summary>
    public IDictionary<string, string>? Hints { get; set; }

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public virtual bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint)
            && !string.IsNullOrWhiteSpace(AccessId)
            && !string.IsNullOrWhiteSpace(SecretAccessKey)
            && !string.IsNullOrWhiteSpace(Project);
    }
}
