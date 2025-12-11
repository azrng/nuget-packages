using Azrng.Core;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.Core.AuditLog;

/// <summary>
/// 日志服务默认实现
/// </summary>
public class DefaultLoggerService : ILoggerService
{
    private readonly ILogger? _logger;
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// 日志服务默认实现
    /// </summary>
    /// <param name="logger">日志</param>
    /// <param name="jsonSerializer"></param>
    public DefaultLoggerService(ILogger? logger, IJsonSerializer? jsonSerializer = null)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer ?? throw new NotImplementedException("请先引用Azrng.Core.Json包");
    }

    /// <summary>
    /// 日志服务默认实现
    /// </summary>
    /// <param name="loggerFactory">日志工厂类</param>
    /// <param name="jsonSerializer"></param>
    public DefaultLoggerService(ILoggerFactory? loggerFactory, IJsonSerializer? jsonSerializer = null)
    {
        _jsonSerializer = jsonSerializer ?? throw new NotImplementedException("请先引用Azrng.Core.Json包");
        if (loggerFactory != null)
        {
            _logger = loggerFactory.CreateLogger<DefaultLoggerService>();
        }
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    public void Write(AuditLogInfo log)
    {
        if (_logger == null)
        {
            return;
        }

        var message = $"TraceId:{log.TraceId},路由:{log.Route},请求方式:{log.HttpMethod}。";

        var logStr = _jsonSerializer.ToJson(log);
        switch (log.LogLevel)
        {
            case LogLevel.Critical:
                _logger.LogCritical("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.Debug:
                _logger.LogDebug("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.Information:
                _logger.LogInformation("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.Error:
                _logger.LogError("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.Warning:
                _logger.LogWarning("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.Trace:
                _logger.LogTrace("{Msg}{@Log}", message, logStr);
                break;
            case LogLevel.None:
            default:
                _logger.LogInformation("{Msg}{@Log}", message, logStr);
                break;
        }
    }

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    public Task WriteAsync(AuditLogInfo log)
    {
        Write(log);
        return Task.CompletedTask;
    }
}