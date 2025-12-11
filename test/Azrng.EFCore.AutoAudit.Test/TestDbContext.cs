using Azrng.EFCore.AutoAudit.Test.Domain;
using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore.AutoAudit.Test;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; init; } = null!;

    public DbSet<Test2Entity> Test2Entities { get; init; } = null!;
}