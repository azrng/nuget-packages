using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DevLogDashboard.Options;

/// <summary>
/// DevLogDashboard 配置选项
/// </summary>
public class DevLogDashboardOptions
{
    private const string DefaultPath = "/dev-logs";

    /// <summary>
    /// 仪表板访问路径
    /// </summary>
    public string EndpointPath { get; set; } = DefaultPath;

    /// <summary>
    /// 最大存储日志条数（默认：10000）
    /// </summary>
    public int MaxLogCount { get; set; } = 10000;

    /// <summary>
    /// 是否只记录错误级别的日志（默认：false，记录所有级别）
    /// </summary>
    public bool OnlyLogErrors { get; set; } = false;

    /// <summary>
    /// 最低日志级别（默认：Trace）
    /// </summary>
    public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 访问授权过滤器
    /// </summary>
    public Func<HttpContext, Task<bool>>? AuthorizationFilter { get; set; }

    /// <summary>
    /// 忽略的路径（不记录日志）
    /// </summary>
    public ICollection<string> IgnoredPaths { get; set; } = new[]
                                                            {
                                                                "/health",
                                                                "/healthz",
                                                                "/ready",
                                                                "/metrics",
                                                                "/dev-logs",
                                                                "/favicon.ico"
                                                            }.ToList();

    /// <summary>
    /// 忽略的 HTTP 方法
    /// </summary>
    public ICollection<string> IgnoredMethods { get; set; } = new[]
                                                              {
                                                                  "OPTIONS"
                                                              }.ToList();

    /// <summary>
    /// 应用名称（可选）
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// 应用版本（可选）
    /// </summary>
    public string? ApplicationVersion { get; set; }
}