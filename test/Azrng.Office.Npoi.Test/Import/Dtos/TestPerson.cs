using Azrng.Office.NPOI.Attributes;

namespace Azrng.Office.Npoi.Test.Import.Dtos;

public class TestPerson
{
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Email { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public bool IsActive { get; set; }

    [IgnoreColumn]
    public string Secret { get; set; } = string.Empty;
}

public class TestPersonWithColumnName
{
    [ColumnName("姓名")]
    public string Name { get; set; } = string.Empty;

    [ColumnName("年龄")]
    public int Age { get; set; }
}

public class SimpleItem
{
    public string Column1 { get; set; } = string.Empty;
    public string Column2 { get; set; } = string.Empty;
    public int Column3 { get; set; }
}

public class NullableItem
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal? Amount { get; set; }
    public bool? IsDeleted { get; set; }
    public Guid? UniqueId { get; set; }
}
