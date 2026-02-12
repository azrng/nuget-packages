namespace Azrng.AspNetCore.Core.AuditLog;

/// <summary>
/// 审计日志中间件配置选项
/// </summary>
public class AuditLogOptions
{
    /// <summary>
    /// 需要忽略的路由前缀
    /// </summary>
    public List<string> IgnoreRoutePrefix { get; set; } = new();

    /// <summary>
    /// 需要记录的 HTTP 方法，空数组表示记录所有方法
    /// 默认记录 POST, PUT, DELETE，不记录 GET
    /// </summary>
    public List<string> IncludeHttpMethods { get; set; } = new() { "POST", "PUT", "DELETE" };

    /// <summary>
    /// 最大响应体大小（字节），超过此大小将截断响应体
    /// 默认 1MB
    /// </summary>
    public int MaxResponseBodySize { get; set; } = 1024 * 1024;

    /// <summary>
    /// 是否只记录 /api/ 开头的请求
    /// </summary>
    public bool LogOnlyApiRoutes { get; set; } = true;

    /// <summary>
    /// 是否格式化 JSON（美化输出）
    /// </summary>
    public bool FormatJson { get; set; } = false;
}