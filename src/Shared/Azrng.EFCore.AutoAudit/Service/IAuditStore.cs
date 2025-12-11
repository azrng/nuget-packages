namespace Azrng.EFCore.AutoAudit.Service;

/// <summary>
/// 审计存储接口
/// </summary>
public interface IAuditStore
{
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="auditEntries"></param>
    /// <returns></returns>
    Task SaveAsync(ICollection<AuditEntryDto> auditEntries);
}