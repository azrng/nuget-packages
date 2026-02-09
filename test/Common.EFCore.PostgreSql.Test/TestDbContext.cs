using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.EFCore.PostgreSql.Test;

/// <summary>
/// 上下文1
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }
}

/// <summary>
/// 上下文2
/// </summary>
public class TestDb2Context : DbContext
{
    public TestDb2Context(DbContextOptions<TestDb2Context> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

[Table("test_table")]
public class TestEntity : IdentityBaseEntity
{
    private TestEntity() { }

    public TestEntity(string content, string name, string? email = null, string? description = null)
    {
        Content = content;
        Name = name;
        Email = email;
        Description = description;
        CreatedTime = DateTimeOffset.UtcNow;
    }

    [Column("content")]
    [StringLength(50)]
    public string? Content { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("created_time")]
    public DateTimeOffset CreatedTime { get; set; }
}