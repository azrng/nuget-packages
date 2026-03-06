using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Models;

/// <summary>
/// 日志条目模型
/// </summary>
public class LogEntry
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 请求唯一标识
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// 连接标识
    /// </summary>
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 日志时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// 日志消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// 日志来源（类名/命名空间）
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 事件 ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// 请求耗时（毫秒）
    /// </summary>
    public double? ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 线程 ID
    /// </summary>
    public int? ThreadId { get; set; }

    /// <summary>
    /// 线程名称
    /// </summary>
    public string? ThreadName { get; set; }

    /// <summary>
    /// 进程 ID
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// 机器名称
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    public string? Application { get; set; }

    /// <summary>
    /// 应用版本
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// 环境名称
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// SDK 版本
    /// </summary>
    public string? SdkVersion { get; set; }

    /// <summary>
    /// Logger 名称
    /// </summary>
    public string? Logger { get; set; }

    /// <summary>
    /// 动作 ID（MVC 场景）
    /// </summary>
    public string? ActionId { get; set; }

    /// <summary>
    /// 动作名称（MVC 场景）
    /// </summary>
    public string? ActionName { get; set; }

    /// <summary>
    /// 扩展属性（结构化日志数据）
    /// </summary>
    public Dictionary<string, object?> Properties { get; set; } = new();

    /// <summary>
    /// 获取所有属性的键值对列表（用于 UI 展示）
    /// </summary>
    public Dictionary<string, object?> GetAllProperties()
    {
        var allProps = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(Timestamp), Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") },
            { nameof(Level), Level.ToString() },
            { nameof(Message), Message },
            { nameof(Source), Source },
            { nameof(RequestId), RequestId },
            { nameof(ConnectionId), ConnectionId },
            { nameof(RequestPath), RequestPath },
            { nameof(RequestMethod), RequestMethod },
            { nameof(ResponseStatusCode), ResponseStatusCode?.ToString() },
            { nameof(ElapsedMilliseconds), ElapsedMilliseconds?.ToString("F2") + "ms" },
            { nameof(ThreadId), ThreadId?.ToString() },
            { nameof(ThreadName), ThreadName },
            { nameof(ProcessId), ProcessId?.ToString() },
            { nameof(MachineName), MachineName },
            { nameof(Application), Application },
            { nameof(AppVersion), AppVersion },
            { nameof(Environment), Environment },
            { nameof(SdkVersion), SdkVersion },
            { nameof(Logger), Logger },
            { nameof(ActionId), ActionId },
            { nameof(ActionName), ActionName },
            { nameof(Exception), Exception },
        };

        // 添加自定义扩展属性
        foreach (var prop in Properties)
        {
            if (!allProps.ContainsKey(prop.Key))
            {
                allProps[prop.Key] = prop.Value;
            }
        }

        return allProps;
    }
}
