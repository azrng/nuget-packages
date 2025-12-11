using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azrng.EFCore.AutoAudit.Test.Domain;

[Table("test")]
public class TestEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")] public string? Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

[Table("test2")]
public class Test2Entity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")] public string? Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}