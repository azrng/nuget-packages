using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Models;

/// <summary>
/// 日志查询条件
/// </summary>
public class LogQuery
{
    /// <summary>
    /// 日志 ID 筛选
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 搜索关键词（支持表达式语法）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 日志级别筛选（最小级别，将筛选出该级别及更高级别的日志）
    /// </summary>
    public LogLevel? MinLevel { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// RequestId 筛选
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 来源筛选
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 应用名称筛选
    /// </summary>
    public string? Application { get; set; }

    /// <summary>
    /// 是否按时间正序排列（默认 false 为倒序）
    /// </summary>
    public bool OrderByTimeAscending { get; set; } = false;

    /// <summary>
    /// 分页索引（从 1 开始）
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// 获取跳过的记录数
    /// </summary>
    public int Skip => (PageIndex - 1) * PageSize;
}
