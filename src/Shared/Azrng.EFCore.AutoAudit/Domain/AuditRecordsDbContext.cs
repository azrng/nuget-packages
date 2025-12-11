using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore.AutoAudit.Domain;

public sealed class AuditRecordsDbContext(DbContextOptions dbContextOptions)
    : DbContext(dbContextOptions)
{
    public DbSet<AuditRecord> AuditRecords { get; set; } = null!;
}