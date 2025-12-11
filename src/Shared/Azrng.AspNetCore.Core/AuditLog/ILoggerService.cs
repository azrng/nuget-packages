namespace Azrng.AspNetCore.Core.AuditLog;

/// <summary>
/// 日志服务接口
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    void Write(AuditLogInfo log);

    /// <summary>
    /// 写日志
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    Task WriteAsync(AuditLogInfo log);
}