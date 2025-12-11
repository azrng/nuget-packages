using Azrng.EFCore.AutoAudit.Domain;

namespace Azrng.EFCore.AutoAudit.Service;

internal sealed class AuditRecordsDbContextStore(AuditRecordsDbContext dbContext) : IAuditStore
{
    public async Task SaveAsync(ICollection<AuditEntryDto> auditEntries)
    {
        if (auditEntries is not { Count: > 0 })
            return;

        foreach (var entry in auditEntries)
        {
            var record = entry.ToAuditRecord();
            await dbContext.AddAsync(record);
        }

        await dbContext.SaveChangesAsync();
    }
}