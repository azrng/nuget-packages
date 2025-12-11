using Azrng.Core.Extension;
using Azrng.EFCore.AutoAudit.Helper;

namespace Azrng.EFCore.AutoAudit.Service;

/// <summary>
/// 审计文件存储
/// </summary>
public class AuditFileStore : IAuditStore
{
    private readonly string _fileName;

    private AuditFileStore(string? fileName)
    {
        _fileName = fileName.GetOrDefault("audits.log");
    }

    public async Task SaveAsync(ICollection<AuditEntryDto> auditEntries)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), _fileName);

        await using var fileStream = File.Exists(path)
            ? new FileStream(path, FileMode.Append)
            : File.Create(path);
        await fileStream.WriteAsync(auditEntries.ToJson().ToBytes());
    }
}