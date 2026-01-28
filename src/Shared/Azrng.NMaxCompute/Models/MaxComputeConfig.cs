namespace Azrng.NMaxCompute.Models;

/// <summary>
/// MaxCompute 配置模型
/// </summary>
public class MaxComputeConfig
{
    /// <summary>
    /// REST API Server 地址
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Access ID
    /// </summary>
    public string AccessId { get; set; } = string.Empty;

    /// <summary>
    /// Secret Key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// JDBC URL（用于连接）
    /// </summary>
    public string JdbcUrl { get; set; } = string.Empty;

    /// <summary>
    /// 最大返回行数
    /// </summary>
    public int MaxRows { get; set; } = 1000;

    /// <summary>
    /// 项目名称
    /// </summary>
    public string? Project { get; set; }

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public virtual bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ServerUrl) &&
               !string.IsNullOrWhiteSpace(AccessId) &&
               !string.IsNullOrWhiteSpace(SecretKey) &&
               !string.IsNullOrWhiteSpace(JdbcUrl);
    }
}
